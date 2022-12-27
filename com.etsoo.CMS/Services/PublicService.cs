﻿using com.etsoo.ApiProxy.Configs;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Public;
using com.etsoo.CoreFramework.Application;
using com.etsoo.DI;
using com.etsoo.SMTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
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
        private readonly IMyApp app;
        private readonly ILogger<ExternalService> logger;
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private readonly IFireAndForgetService fireService;
        private readonly IPAddress ip;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="logger">Logger</param>
        /// <param name="clientFactory">Client factory</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="fireService">Fire service</param>
        public PublicService(IMyApp app, ILogger<ExternalService> logger, IHttpClientFactory clientFactory, IConfiguration configuration, IFireAndForgetService fireService, IHttpContextAccessor httpContextAccessor)
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
            this.configuration = configuration;
            this.fireService = fireService;
        }

        /// <summary>
        /// Async create WeiXin JS siganture
        /// 异步创建微信 Js 接口签名
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Json data</returns>
        public async Task<WXJsApiSignatureResult> CreateJsApiSignatureAsync(CreateJsApiSignatureRQ rq)
        {
            // Repo
            var repo = new WebsiteRepo(app, null);

            // Plugin
            var wx = await repo.ReadServiceAsync(WXClientOptions.Name);

            if (wx == null)
            {
                throw new NotSupportedException("WeiXin Client Not Supported");
            }

            // Options
            var secret = app.DecriptData(wx.Secret);
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
        /// <returns>Result</returns>
        public async Task<IActionResult> SendEmailAsync(SendEmailRQ rq)
        {
            // Repo
            var repo = new WebsiteRepo(app, null);

            // Plugin
            var recap = await repo.ReadServiceAsync(RecaptchaOptions.Name);

            // Valid token
            if (recap != null)
            {
                string? baseAddress = null;
                var secret = app.DecriptData(recap.Secret);
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
                var result = await recapApi.SiteVerifyAsync(new()
                {
                    Response = rq.Token,
                    RemoteIp = ip.ToString()
                });
                if (!result.Success)
                {
                    logger.LogDebug("SiteVerifyAsync {@result}", result);
                    return ApplicationErrors.AccessDenied.AsResult("Token");
                }
            }

            // Validation
            if (!MailboxAddress.TryParse(rq.Recipient, out var to))
            {
                return ApplicationErrors.InvalidEmail.AsResult("Recipient");
            }

            var smtp = await repo.ReadServiceAsync("SMTP");
            if (smtp == null)
            {
                return ApplicationErrors.NoValidData.AsResult("SMTP");
            }

            var smtpJSON = app.DecriptData(smtp.Secret);
            var smtpOptions = ServiceUtils.ParseOptions<SMTPClientOptions>(smtpJSON);
            if (smtpOptions == null)
            {
                return ApplicationErrors.NoValidData.AsResult("SMTPOptions");
            }

            // Template
            using var jsonDoc = JsonDocument.Parse(smtpJSON);
            var template = jsonDoc.RootElement.GetPropertyCaseInsensitive(rq.Template)?.GetString();
            if (string.IsNullOrEmpty(template))
            {
                return ApplicationErrors.NoValidData.AsResult("Template");
            }

            // Parse data
            using var doc = JsonDocument.Parse(rq.Data);
            foreach (var item in doc.RootElement.EnumerateObject())
            {
                template = template.Replace($"{{{item.Name}}}", HttpUtility.HtmlDecode(item.Value.ToString()));
            }

            // Message
            var message = new MimeMessage
            {
                Subject = HttpUtility.HtmlDecode(rq.Subject)
            };

            // Configure "Bcc" to make sure the email is received by somebody else
            message.To.Add(to);

            // HTML
            var builder = new BodyBuilder();

            var html = new StringBuilder("""
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
            html.Append(template);
            html.Append("""
                    </body>
                </html>
            """);

            builder.HtmlBody = html.ToString();

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

            return ActionResult.Success;
        }
    }
}