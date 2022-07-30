using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Services;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.UserAgentParser;
using com.etsoo.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace com.etsoo.CMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : SharedController
    {
        // Service
        readonly AuthService service;

        // Accessor
        readonly IHttpContextAccessor httpContextAccessor;

        // User agent
        readonly string? userAgent;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="logger">Logger</param>
        public AuthController(IMyApp app, IHttpContextAccessor httpContextAccessor, ILogger<AuthController> logger)
            : base(app)
        {
            this.httpContextAccessor = httpContextAccessor;
            userAgent = httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.UserAgent].ToString();
            service = new AuthService(app, logger);
        }

        /// <summary>
        /// Login
        /// 登录
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Task</returns>
        [HttpPost("Login")]
        public async Task Login(LoginRQ model)
        {
            // IP address
            var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            if (ip == null)
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("IP"));
                return;
            }

            var parseResult = ParseUserAgent(userAgent, out UAParser parser);
            if (!parseResult.Ok)
            {
                await WriteResultAsync(parseResult);
                return;
            }

            // Get device core
            var device = parser.ToShortName();
            var deviceCore = service.DecryptDeviceCore(model.DeviceId, device);
            if (string.IsNullOrEmpty(deviceCore))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Device"));
                return;
            }

            // Decrypt
            var id = service.DecryptDeviceData(model.Id, deviceCore);
            if (string.IsNullOrEmpty(id))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Id"));
                return;
            }

            var pwd = service.DecryptDeviceData(model.Pwd, deviceCore);
            if (string.IsNullOrEmpty(pwd))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Pwd"));
                return;
            }

            // Data
            var data = new LoginDto(id, pwd, ip);

            // Login
            var (result, token) = await service.LoginAsync(data);
        }

        /// <summary>
        /// Init call
        /// 初始化调用
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        [HttpPut("WebInitCall")]
        public async Task WebInitCall(InitCallRQ rq)
        {
            var result = ParseUserAgent(userAgent, out string identifier);
            if (!result.Ok)
            {
                await WriteResultAsync(result);
                return;
            }

            // Result
            var initResult = await service.WebInitCallAsync(rq, identifier);

            await WriteResultAsync(initResult);
        }
    }
}
