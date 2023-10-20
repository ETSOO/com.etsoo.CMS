﻿using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Website service interface
    /// 网站业务逻辑服务接口
    /// </summary>
    public interface IWebsiteService : IServiceBase
    {
        /// <summary>
        /// Create or update resource
        /// 创建或更新资源
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="ip">IP address</param>
        /// <returns>Action result</returns>
        Task<IActionResult> CreateOrUpdateResourceAsync(ResourceCreateRQ rq, IPAddress ip);

        /// <summary>
        /// Create service
        /// 创建服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> CreateServiceAsync(ServiceCreateRQ rq, IPAddress ip);

        /// <summary>
        /// Dashboard data
        /// 仪表盘数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task DashboardAsync(HttpResponse response);

        /// <summary>
        /// Initialize website
        /// 初始化网站
        /// </summary>
        /// <param name="rq">Reqeust data</param>
        /// <returns>Task</returns>
        Task<IActionResult> InitializeAsync(InitializeRQ rq);

        /// <summary>
        /// Async on demand revalidation
        /// 异步按需重新验证
        /// </summary>
        /// <param name="urls">URLs</param>
        /// <returns>Task</returns>
        ValueTask<IActionResult> OnDemandRevalidateAsync(params string[] urls);

        /// <summary>
        /// Get mobile QRCode image Base64 string
        /// 获取移动端QRCode图片的Base64字符串
        /// </summary>
        /// <returns>Base64 string</returns>
        Task<string> QRCodeAsync(string url);

        /// <summary>
        /// Read JSON data
        /// 读取 JSON 数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task ReadJsonDataAsync(HttpResponse response);

        /// <summary>
        /// Read service (plugin)
        /// 读取服务（插件）
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        Task<DbService> ReadServiceAsync(string id);

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task ReadSettingsAsync(HttpResponse response);

        /// <summary>
        /// Regenerate all tab URLs
        /// 重新生成所有栏目网址
        /// </summary>
        /// <returns>Result</returns>
        ValueTask<IActionResult> RegenerateTabUrlsAsync();

        /// <summary>
        /// Query resources
        /// 查询资源
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task QueryResourcesAsync(HttpResponse response);

        /// <summary>
        /// Query plugin services
        /// 查询插件服务
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task QueryServicesAsync(HttpResponse response);

        /// <summary>
        /// Update resource URL
        /// 更新资源路径
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        Task<IActionResult> UpdateResurceUrlAsync(WebsiteUpdateResurceUrlRQ rq, IPAddress ip);

        /// <summary>
        /// Update settings
        /// 更新设置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns></returns>
        Task<IActionResult> UpdateSettingsAsync(WebsiteUpdateSettingsRQ rq, IPAddress ip);

        /// <summary>
        /// Update service
        /// 更新服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> UpdateServiceAsync(ServiceUpdateRQ rq, IPAddress ip);

        /// <summary>
        /// Upgrade system
        /// 升级系统
        /// </summary>
        /// <returns>Task</returns>
        Task<IActionResult> UpgradeSystemAsync();
    }
}
