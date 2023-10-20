using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.HTTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website tab service
    /// 网站栏目业务逻辑服务
    /// </summary>
    public class TabService : CommonService<TabRepo>, ITabService
    {
        readonly IStorage storage;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        public TabService(IMyApp app, IServiceUserAccessor userAccessor, ILogger<TabService> logger, IStorage storage)
            : base(app, new TabRepo(app, userAccessor.UserSafe), logger)
        {
            this.storage = storage;
        }

        /// <summary>
        /// Create tab
        /// 创建栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(TabCreateRQ rq, IPAddress ip)
        {
            var result = await Repo.CreateAsync(rq);
            var id = result.Data;
            await Repo.AddAuditAsync(AuditKind.CreateTab, id.ToString(), $"Create tab {id}", ip, result.Result, rq);
            return result.MergeData("id");
        }

        /// <summary>
        /// Delete tab
        /// 删除栏目
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(int id)
        {
            return await Repo.DeleteAsync(id);
        }

        /// <summary>
        /// List
        /// 列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task ListAsync(TiplistRQ<int> rq, HttpResponse response)
        {
            await Repo.ListAsync(rq, response);
        }

        /// <summary>
        /// Query tab
        /// 查询栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(TabQueryRQ rq, HttpResponse response)
        {
            await Repo.QueryAsync(rq, response);
        }

        /// <summary>
        /// Sort data
        /// 数据排序
        /// </summary>
        /// <param name="sortData">Data to sort</param>
        /// <returns>Rows affected</returns>
        public async Task<int> SortAsync(Dictionary<int, short> sortData)
        {
            return await Repo.SortAsync(sortData);
        }

        /// <summary>
        /// Update tab
        /// 更新栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(TabUpdateRQ rq, IPAddress ip)
        {
            if (!string.IsNullOrEmpty(rq.Url))
            {
                rq.Url = rq.Url.TrimEnd('/');
            }

            var refreshTime = DateTime.UtcNow.ToString("u");
            var parameters = new Dictionary<string, object>
            {
                [nameof(refreshTime)] = refreshTime
            };

            var (result, _) = await Repo.InlineUpdateAsync<int, UpdateModel<int>>(
                rq,
                new QuickUpdateConfigs(new[] { "parent", "name", "description", "jsonData", "logo", "icon", "url", "layout", "status AS enabled=IIF(@Enabled, 0, 200)" })
                {
                    TableName = "tabs",
                    IdField = "id"
                },
                $"refreshTime = @{nameof(refreshTime)}", parameters
             );
            await Repo.AddAuditAsync(AuditKind.UpdateTab, rq.Id.ToString(), $"Update tab {rq.Id}", ip, result, rq);
            return result;
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <param name="logoStream">Logo stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="ip">IP</param>
        /// <returns>New URL</returns>
        public async ValueTask<string?> UploadLogoAsync(int id, Stream logoStream, string contentType, IPAddress ip)
        {
            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            var logo = await Repo.ReadLogoAsync(id);
            if (logo == null)
            {
                return null;
            }

            // File path
            var path = $"/Resources/TabLogos/t{id.ToString().PadLeft(6, '0')}.{Path.GetRandomFileName()}{extension}";

            // Save the stream to file directly
            var saveResult = await storage.WriteAsync(path, logoStream, WriteCase.CreateNew);

            if (saveResult)
            {
                // New avatar URL
                var url = storage.GetUrl(path);

                // Repo update
                if (await Repo.UpdateLogoAsync(id, url) > 0)
                {
                    // Audit
                    await Repo.AddAuditAsync(AuditKind.UpdateTabLogo, id.ToString(), $"Update tab {id} logo", new { Logo = logo, NewLogo = url }, ip);

                    // Return
                    return url;
                }
            }

            Logger.LogError("Logo write path is {path}", path);

            await Repo.AddAuditAsync(AuditKind.UpdateTabLogo, id.ToString(), $"Update tab {id} logo", ip, new ActionResult(), new { Path = path });

            return null;
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(int id, HttpResponse response)
        {
            await Repo.UpdateReadAsync(response, id);
        }

        /// <summary>
        /// Read for ancestors
        /// 上层栏目浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task AncestorReadAsync(int id, HttpResponse response)
        {
            await Repo.AncestorReadAsync(response, id);
        }
    }
}