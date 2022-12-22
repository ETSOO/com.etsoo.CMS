using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website service
    /// 网站业务逻辑服务
    /// </summary>
    public class WebsiteService : CommonService<WebsiteRepo>, IWebsiteService
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        public WebsiteService(IMyApp app, IServiceUserAccessor userAccessor, ILogger<WebsiteService> logger)
            : base(app, new WebsiteRepo(app, userAccessor.UserSafe), logger)
        {
        }

        /// <summary>
        /// Create or update resource
        /// 创建或更新资源
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="ip">IP address</param>
        /// <returns>Action result</returns>
        public async Task<IActionResult> CreateOrUpdateResourceAsync(ResourceCreateRQ rq, IPAddress ip)
        {
            var result = await Repo.CreateOrUpdateResourceAsync(rq);

            await Repo.AddAuditAsync(AuditKind.CreateResource, rq.Id, $"Create or update resource {rq.Id}", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Create service
        /// 创建服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateServiceAsync(ServiceCreateRQ rq, IPAddress ip)
        {
            var secret = rq.Secret;
            if (!string.IsNullOrEmpty(secret))
            {
                rq.Secret = App.EncriptData(secret);
                secret = StringUtils.HideData(secret);
            }

            var result = await Repo.CreateServiceAsync(rq);

            rq.Secret = secret;
            await Repo.AddAuditAsync(AuditKind.CreateService, rq.Id, $"Create service {rq.Id}", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Dashboard data
        /// 仪表盘数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task DashboardAsync(HttpResponse response)
        {
            await Repo.DashboardAsync(response);
        }

        /// <summary>
        /// Initialize website
        /// 初始化网站
        /// </summary>
        /// <param name="rq">Reqeust data</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> InitializeAsync(InitializeRQ rq)
        {
            return await Repo.InitializeAsync(rq);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task ReadSettingsAsync(HttpResponse response)
        {
            await Repo.ReadSettingsAsync(response);
        }

        /// <summary>
        /// Query resources
        /// 查询资源
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryResourcesAsync(HttpResponse response)
        {
            await Repo.QueryResourcesAsync(response);
        }

        /// <summary>
        /// Query plugin services
        /// 查询插件服务
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryServicesAsync(HttpResponse response)
        {
            await Repo.QueryServicesAsync(response);
        }

        /// <summary>
        /// Update settings
        /// 更新设置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateSettingsAsync(WebsiteUpdateSettingsRQ rq, IPAddress ip)
        {
            if (!string.IsNullOrEmpty(rq.Domain))
            {
                rq.Domain = rq.Domain.TrimEnd('/');
            }

            if (!string.IsNullOrEmpty(rq.Keywords))
            {
                rq.Keywords = ServiceUtils.FormatKeywords(rq.Keywords);
            }

            // View website
            var data = await Repo.ReadSettingsAsync();
            if (data == null)
            {
                var result = await Repo.CreateSettingsAsync(rq);
                await Repo.AddAuditAsync(AuditKind.UpdateWebsiteSettings, rq.Id.ToString(), $"Create Website Settings", ip, result, rq);
                return result;
            }
            else
            {
                var newRQ = rq with { Id = data.RowId };
                var (result, _) = await Repo.InlineUpdateAsync<int, UpdateModel<int>>(newRQ, new QuickUpdateConfigs(new[] { "Domain", "Title", "Description", "Keywords" })
                {
                    TableName = "website",
                    IdField = "rowid"
                });

                // Audit Json
                var json = result.Ok && rq.ChangedFields != null ? await SharedUtils.JoinAsAuditJsonAsync(data, newRQ, rq.ChangedFields) : null;

                await Repo.AddAuditAsync(AuditKind.UpdateWebsiteSettings, newRQ.Id.ToString(), $"Update Website Settings", ip, result, json);
                return result;
            }
        }

        /// <summary>
        /// Update service
        /// 更新服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateServiceAsync(ServiceUpdateRQ rq, IPAddress ip)
        {
            var secret = rq.Secret;
            if (!string.IsNullOrEmpty(secret))
            {
                rq.Secret = App.EncriptData(secret);
                secret = StringUtils.HideData(secret);
            }

            var parameters = new Dictionary<string, object>
            {
                ["RefreshTime"] = DateTime.UtcNow.ToString("u")
            };
            var (result, _) = await Repo.InlineUpdateAsync(rq, new QuickUpdateConfigs(new[] { "app", "secret", "status AS enabled=IIF(@Enabled, 0, 200)" })
            {
                TableName = "services",
                IdField = "id"
            }, "refreshTime = @RefreshTime", parameters);

            rq.Secret = secret;
            await Repo.AddAuditAsync(AuditKind.UpdateService, rq.Id, $"Update service {rq.Id}", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Upgrade system
        /// 升级系统
        /// </summary>
        /// <returns>Task</returns>
        public async Task<IActionResult> UpgradeSystemAsync()
        {
            return await Repo.UpgradeSystemAsync();
        }
    }
}