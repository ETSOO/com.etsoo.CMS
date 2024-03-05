using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.ServiceApp.Application;
using com.etsoo.Utils;
using com.etsoo.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace com.etsoo.CMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : SharedController
    {
        // Service
        readonly IAuthService service;
        readonly ILogger<AuthController> logger;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="service">Service</param>
        /// <param name="logger">Logger</param>
        public AuthController(IMyApp app, IHttpContextAccessor httpContextAccessor, IAuthService service, ILogger<AuthController> logger)
            : base(app, httpContextAccessor)
        {
            this.service = service;
            this.logger = logger;
        }

        /// <summary>
        /// Log frontend error
        /// 记录前端错误
        /// </summary>
        /// <param name="rq">Rquest data</param>
        [HttpPost("LogFrontendError")]
        public async Task LogFrontendError(ErrorLogData rq)
        {
            await using var stream = SharedUtils.GetStream();
            await JsonSerializer.SerializeAsync(stream, rq, ModelJsonSerializerContext.Default.ErrorLogData, CancellationToken);
            var json = Encoding.UTF8.GetString(stream.ToArray());
            logger.LogWarning(json);
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
            // Check device
            if (!CheckDevice(service, model.DeviceId, out var checkResult, out var cd))
            {
                await WriteResultAsync(checkResult);
                return;
            }
            var deviceCore = cd.Value.DeviceCore;

            // Decrypt
            var id = service.DecryptDeviceData(model.Id, deviceCore);
            if (string.IsNullOrEmpty(id))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Id"));
                return;
            }

            // Ignore case
            id = id.ToLower();

            var pwd = service.DecryptDeviceData(model.Pwd, deviceCore);
            if (string.IsNullOrEmpty(pwd))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Pwd"));
                return;
            }

            // Data
            var data = new LoginDto(id, pwd, Ip, deviceCore, model.Region, model.Timezone);

            // Login
            var (result, token) = await service.LoginAsync(data, CancellationToken);

            // Pass the token through header
            if (token != null)
                WriteHeader(Constants.RefreshTokenHeaderName, token);

            // Output
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Refresh token
        /// 刷新令牌
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Task</returns>
        [HttpPut("RefreshToken")]
        public async Task RefreshToken(RefreshTokenRQ model)
        {
            // Check device
            if (!CheckDevice(service, model.DeviceId, out var checkResult, out var cd))
            {
                await WriteResultAsync(checkResult);
                return;
            }
            var deviceCore = cd.Value.DeviceCore;

            // Token
            string? token;
            if (!Request.Headers.TryGetValue(Constants.RefreshTokenHeaderName, out var value) || string.IsNullOrEmpty((token = value.ToString())))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult());
                return;
            }

            // Decrypt
            token = service.DecryptDeviceData(token, deviceCore);
            if (string.IsNullOrEmpty(token))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Token"));
                return;
            }

            string? pwd = null;
            if (!string.IsNullOrEmpty(model.Pwd))
            {
                pwd = service.DecryptDeviceData(model.Pwd, deviceCore);
                if (string.IsNullOrEmpty(pwd))
                {
                    await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Pwd"));
                    return;
                }
            }

            // Result & refresh token
            var (result, refreshToken) = await service.RefreshTokenAsync(token, new RefreshTokenDto(deviceCore, Ip, pwd, model.Timezone), CancellationToken);

            // Pass the token through header
            if (refreshToken != null)
                WriteHeader(Constants.RefreshTokenHeaderName, refreshToken);

            // Output
            await WriteResultAsync(result);
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
            // Device check
            if (!CheckDevice(out var checkResult, out var parser))
            {
                await WriteResultAsync(checkResult);
                return;
            }

            // Result
            var initResult = await service.WebInitCallAsync(rq, parser.ToShortName());

            await WriteResultAsync(initResult);
        }
    }
}
