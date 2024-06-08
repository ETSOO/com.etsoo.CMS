using com.etsoo.ApiProxy.Configs;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Server.Defs;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.DI;
using com.etsoo.SMTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
using com.etsoo.Web;
using com.etsoo.WeiXin;
using com.etsoo.WeiXin.Dto;
using com.etsoo.WeiXin.RQ;
using MimeKit;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Public service
    /// 公共服务
    /// </summary>
    public class PublicService : IPublicService
    {
        readonly IMyApp app;
        readonly ILogger<ExternalService> logger;
        readonly IHttpClientFactory clientFactory;
        readonly IFireAndForgetService fireService;
        readonly IPAddress ip;
        readonly IPublicCommonService publicService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="logger">Logger</param>
        /// <param name="clientFactory">Client factory</param>
        /// <param name="fireService">Fire service</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="websiteService">Website service</param>
        public PublicService(IMyApp app, ILogger<ExternalService> logger, IHttpClientFactory clientFactory, IFireAndForgetService fireService, IHttpContextAccessor httpContextAccessor, IPublicCommonService publicService)
        {
            var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            if (ip == null)
            {
                throw new NullReferenceException(nameof(ip));
            }
            this.ip = ip;

            this.app = app;
            this.logger = logger;
            this.clientFactory = clientFactory;
            this.fireService = fireService;
            this.publicService = publicService;
        }

        /// <summary>
        /// Async create WeiXin JS siganture
        /// 异步创建微信 Js 接口签名
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Json data</returns>
        public async Task<WXJsApiSignatureResult> CreateJsApiSignatureAsync(CreateJsApiSignatureRQ rq, CancellationToken cancellationToken = default)
        {
            // Plugin
            var wx = await publicService.ReadServiceAsync(WXClientOptions.Name, cancellationToken) ?? throw new NotSupportedException("WeiXin Client Not Supported");

            // Options
            var secret = wx.Secret;
            var options = ServiceUtils.ParseOptions<WXClientOptions>(secret) ?? new WXClientOptions
            {
                AppId = wx.App,
                AppSecret = secret
            };

            // Client
            var client = clientFactory.CreateClient();
            var wxApi = new WXClient(client, options);

            return await wxApi.CreateJsApiSignatureAsync(rq.Url);
        }

        /// <summary>
        /// Async send email
        /// 异步发送邮件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> SendEmailAsync(SendEmailRQ rq, CancellationToken cancellationToken = default)
        {
            // Plugin
            var recap = await publicService.ReadServiceAsync(RecaptchaOptions.Name, cancellationToken);

            // Valid token
            if (recap != null)
            {
                string? baseAddress = null;
                var secret = recap.Secret;
                var options = ServiceUtils.ParseOptions<RecaptchaOptions>(secret);
                if (options != null)
                {
                    baseAddress = options.BaseAddress;
                    secret = options.Secret;
                }

                var client = clientFactory.CreateClient();
                var recapApi = new RecaptchaProxy(client, logger, new RecaptchaOptions
                {
                    BaseAddress = baseAddress,
                    Secret = secret
                });

                var verifyResult = await recapApi.SiteVerifyAsync(new()
                {
                    Response = rq.Token,
                    RemoteIp = ip.ToString()
                }, cancellationToken);

                if (!verifyResult.Success)
                {
                    logger.LogDebug("SiteVerifyAsync {@result}", verifyResult);
                    return ApplicationErrors.AccessDenied.AsResult("Token");
                }
            }

            // Validation
            if (!MailboxAddress.TryParse(rq.Recipient, out var to))
            {
                return ApplicationErrors.InvalidEmail.AsResult("Recipient");
            }

            var smtp = await publicService.ReadServiceAsync("SMTP", cancellationToken);
            if (smtp == null)
            {
                return ApplicationErrors.NoValidData.AsResult("SMTP");
            }

            // SMTP options
            var smtpJSON = smtp.Secret;
            var smtpOptions = ServiceUtils.ParseOptions<SMTPClientOptions>(smtpJSON);
            if (smtpOptions == null)
            {
                return ApplicationErrors.NoValidData.AsResult("SMTPOptions");
            }

            // Template
            var templateText = await publicService.QueryResourceAsync(rq.Template, cancellationToken);
            if (string.IsNullOrEmpty(templateText))
            {
                return ApplicationErrors.NoValidData.AsResult("Template");
            }

            var template = JsonSerializer.Deserialize(templateText, ModelJsonSerializerContext.Default.EmailTemplateDto);
            if (template == null)
            {
                return ApplicationErrors.NoValidData.AsResult("Template");
            }

            // Parse data
            string html;
            if (template.IsRazor is true)
            {
                html = await RazorUtils.RenderAsync(rq.Template, template.Template, rq.Data);
            }
            else
            {
                var sb = new StringBuilder("""
                <!doctype html>
                <html>
                    <head>
                        <meta
                          http-equiv="Content-Security-Policy"
                          content="default-src 'self' data: 'unsafe-inline'"
                        />
                        <style>
                            body{font-size: 12pt;}
                        </style>
                    </head>
                    <body>
                """);
                sb.Append(template.Template.FormatTemplateWithJson(rq.Data, "(empty)"));
                sb.Append("""
                    </body>
                </html>
                """);
                html = sb.ToString();
            }

            // Message
            var message = new MimeMessage
            {
                Subject = HttpUtility.HtmlDecode(template.Subject)
            };

            // Recipient
            message.To.Add(to);

            // Configure "Cc", "Bcc" with the template or SMTP options to make sure the email is received by somebody else
            message.Cc.AddRange(template.Cc);
            message.Bcc.AddRange(template.Bcc);

            // HTML
            var builder = new BodyBuilder
            {
                HtmlBody = html.ToString()
            };

            message.Body = builder.ToMessageBody();

            // Fire and forget
            fireService.FireAsync(async (logger) =>
            {
                try
                {
                    // SMTP client
                    var smtpClient = new SMTPClient(smtpOptions);

                    // Send
                    await smtpClient.SendAsync(message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Email Sending Exception / 邮件发送异常 {@rq}", rq);
                }
            });

            var result = ActionResult.Success;

            if (!string.IsNullOrEmpty(template.SuccessMessage))
                result.Data["SuccessMessage"] = template.SuccessMessage;

            return result;
        }
    }
}