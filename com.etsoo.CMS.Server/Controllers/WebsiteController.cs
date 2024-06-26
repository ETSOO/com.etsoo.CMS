﻿using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Web;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        readonly IWebsiteService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="service">Service</param>
        public WebsiteController(IMyApp app, IHttpContextAccessor httpContextAccessor, IWebsiteService service)
            : base(app, httpContextAccessor)
        {
            this.service = service;
        }

        /// <summary>
        /// Create or update resource
        /// 创建或更新资源
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("CreateOrUpdateResource")]
        public async Task CreateOrUpdateResource(ResourceCreateRQ rq)
        {
            var result = await service.CreateOrUpdateResourceAsync(rq, CancellationToken);
            await WriteResultAsync(result);
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

            var result = await service.CreateServiceAsync(rq, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Dashboard data
        /// 仪表盘数据
        /// </summary>
        /// <returns>Task</returns>
        [HttpGet("Dashboard")]
        public async Task Dashboard()
        {
            await service.DashboardAsync(Response, CancellationToken);
        }

        /// <summary>
        /// Initialize website
        /// 初始化网站
        /// </summary>
        /// <returns>Task</returns>
        [HttpPost("Initialize")]
        public async Task Initialize(InitializeRQ rq)
        {
            var result = await service.InitializeAsync(rq, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Get mobile QRCode image Base64 string
        /// 获取移动端QRCode图片的Base64字符串
        /// </summary>
        /// <returns>Base64 string</returns>
        [HttpGet("QRCode")]
        public async Task<string> QRCode()
        {
            var request = context.HttpContext?.Request;
            if (request == null) return string.Empty;

            var url = $"{request.Scheme}://{request.Host}";

            if (request.Path.Value?.StartsWith("/cms/", StringComparison.OrdinalIgnoreCase) is true) url += "/cms/";

            url += "?loginid={id}";

            return await service.QRCodeAsync(url, CancellationToken);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <returns>Task</returns>
        [HttpGet("ReadJsonData")]
        public async Task ReadJsonData()
        {
            await service.ReadJsonDataAsync(Response, CancellationToken);
        }

        /// <summary>
        /// Read service (plugin)
        /// 读取服务（插件）
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        [HttpGet("ReadService/{id}")]
        public async Task<IActionResult> ReadService([Required] string id)
        {
            return new JsonResult(await service.ReadServiceAsync(id, CancellationToken));
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <returns>Task</returns>
        [HttpGet("ReadSettings")]
        public async Task ReadSettings()
        {
            await service.ReadSettingsAsync(Response, CancellationToken);
        }

        /// <summary>
        /// Regenerate all tab URLs
        /// 重新生成所有栏目网址
        /// </summary>
        /// <returns>Task</returns>
        [HttpPut("RegenerateTabUrls")]
        public async Task RegenerateTabUrls()
        {
            var result = await service.RegenerateTabUrlsAsync(CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Regenerate URL
        /// 重新生成网址
        /// </summary>
        /// <returns>Task</returns>
        [HttpPost("RegenerateUrl")]
        public async Task RegenerateUrl(IEnumerable<string> urls)
        {
            var result = await service.OnDemandRevalidateAsync(urls.ToArray(), CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Query resources
        /// 查询资源
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        [HttpPost("QueryResources")]
        public async Task QueryResources()
        {
            await service.QueryResourcesAsync(Response, CancellationToken);
        }

        /// <summary>
        /// Query article JSON data schema
        /// 查询文章JSON数据模式
        /// </summary>
        /// <returns></returns>
        [HttpGet("QueryArticleJsonDataSchema")]
        public async Task QueryArticleJsonDataSchema()
        {
            await service.QueryArticleJsonDataSchemaAsync(Response, CancellationToken);
        }

        /// <summary>
        /// Query tab JSON data schema
        /// 查询栏目JSON数据模式
        /// </summary>
        /// <returns></returns>
        [HttpGet("QueryTabJsonDataSchema")]
        public async Task QueryTabJsonDataSchema()
        {
            await service.QueryTabJsonDataSchemaAsync(Response, CancellationToken);
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
            await service.QueryServicesAsync(Response, CancellationToken);
        }

        /// <summary>
        /// Update resource URL
        /// 更新资源路径
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("UpdateResourceUrl")]
        public async Task UpdateResourceUrl(WebsiteUpdateResurceUrlRQ rq)
        {
            var result = await service.UpdateResourceUrlAsync(rq, CancellationToken);
            await WriteResultAsync(result);
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
            var result = await service.UpdateSettingsAsync(rq, CancellationToken);
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

            var result = await service.UpdateServiceAsync(rq, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Upgrade system
        /// 升级系统
        /// </summary>
        /// <returns>Task</returns>
        [HttpPut("UpgradeSystem")]
        public async Task UpgradeSystem()
        {
            var result = await service.UpgradeSystemAsync(CancellationToken);
            await WriteResultAsync(result);
        }
    }
}
