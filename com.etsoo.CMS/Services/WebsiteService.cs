using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.Services;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using System.Net;
using System.Text.RegularExpressions;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website service
    /// 网站业务逻辑服务
    /// </summary>
    public class WebsiteService : SqliteService<WebsiteRepo>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <param name="logger">Logger</param>
        public WebsiteService(IMyApp app, IServiceUser user, ILogger logger)
            : base(app, new WebsiteRepo(app, user), logger)
        {
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
            var result = await Repo.CreateServiceAsync(rq);

            if (!string.IsNullOrEmpty(rq.Secret))
                rq.Secret = StringUtils.HideData(rq.Secret);
            await Repo.AddAuditAsync(AuditKind.CreateService, rq.Id, $"Create service {rq.Id}", ip, result, rq);

            return result;
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
            if (!string.IsNullOrEmpty(rq.Keywords))
            {
                // Unify the format
                var items = new Regex(@"\s*[;；,，]+\s*", RegexOptions.Multiline).Split(rq.Keywords);
                rq.Keywords = string.Join(", ", items);
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
            var parameters = new Dictionary<string, object>
            {
                ["RefreshTime"] = DateTime.UtcNow.ToString("s")
            };
            var (result, _) = await Repo.InlineUpdateAsync(rq, new QuickUpdateConfigs(new[] { "app", "secret", "status AS enabled=IIF(@Enabled, 0, 200)" })
            {
                TableName = "services",
                IdField = "id"
            }, "refreshTime = @RefreshTime", parameters);

            if (!string.IsNullOrEmpty(rq.Secret))
                rq.Secret = StringUtils.HideData(rq.Secret);
            await Repo.AddAuditAsync(AuditKind.UpdateService, rq.Id, $"Update service {rq.Id}", ip, result, rq);

            return result;
        }
    }
}