using com.etsoo.CMS.Application;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CMS.Services;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.User;
using com.etsoo.Web;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Website controller
    /// 网站控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WebsiteController : SharedController
    {
        // Service
        readonly WebsiteService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="logger">Logger</param>
        public WebsiteController(IMyApp app, IHttpContextAccessor httpContextAccessor, ILogger<WebsiteController> logger)
            : base(app, httpContextAccessor)
        {
            service = new WebsiteService(app, ServiceUser.CreateSafe(httpContextAccessor.HttpContext), logger);
        }

        /// <summary>
        /// Create plugin service
        /// 创建插件服务
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("CreateService")]
        public async Task CreateService(ServiceCreateRQ rq)
        {
            // Check device
            if (!CheckDevice(service, rq.DeviceId, out var checkResult, out var cd))
            {
                await WriteResultAsync(checkResult);
                return;
            }
            var deviceCore = cd.Value.DeviceCore;

            if (!string.IsNullOrEmpty(rq.Secret))
            {
                var secret = service.DecryptDeviceData(rq.Secret, deviceCore);
                if (string.IsNullOrEmpty(secret))
                {
                    await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Secret"));
                    return;
                }
                rq.Secret = secret;
            }

            var result = await service.CreateServiceAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <returns>Task</returns>
        [HttpGet("ReadSettings")]
        public async Task ReadSettings()
        {
            await service.ReadSettingsAsync(Response);
        }

        /// <summary>
        /// Query plugin services
        /// 查询插件服务
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        [HttpPost("QueryServices")]
        public async Task QueryServices()
        {
            await service.QueryServicesAsync(Response);
        }

        /// <summary>
        /// Update settings
        /// 更新设置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns></returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("UpdateSettings")]
        public async Task UpdateSettings(WebsiteUpdateSettingsRQ rq)
        {
            var result = await service.UpdateSettingsAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Update service
        /// 更新服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("UpdateService")]
        public async Task UpdateService(ServiceUpdateRQ rq)
        {
            // Check device
            if (!CheckDevice(service, rq.DeviceId, out var checkResult, out var cd))
            {
                await WriteResultAsync(checkResult);
                return;
            }
            var deviceCore = cd.Value.DeviceCore;

            if (!string.IsNullOrEmpty(rq.Secret))
            {
                var secret = service.DecryptDeviceData(rq.Secret, deviceCore);
                if (string.IsNullOrEmpty(secret))
                {
                    await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Secret"));
                    return;
                }
                rq.Secret = secret;
            }

            var result = await service.UpdateServiceAsync(rq, Ip);
            await WriteResultAsync(result);
        }
    }
}
