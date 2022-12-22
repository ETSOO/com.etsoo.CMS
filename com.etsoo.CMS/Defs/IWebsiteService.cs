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
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task ReadSettingsAsync(HttpResponse response);

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
