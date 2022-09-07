using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website tab service
    /// 网站栏目业务逻辑服务
    /// </summary>
    public class TabService : SqliteService<TabRepo>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <param name="logger">Logger</param>
        public TabService(IMyApp app, IServiceUser user, ILogger logger)
            : base(app, new TabRepo(app, user), logger)
        {
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
        /// Delete single user
        /// 删除单个用户
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Action result</returns>
        public virtual async ValueTask<ActionResult> DeleteAsync(int id)
        {
            return await Repo.DeleteAsync(id);
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
            var refreshTime = DateTime.UtcNow.ToString("s");
            var parameters = new Dictionary<string, object>
            {
                [nameof(refreshTime)] = refreshTime
            };

            var (result, _) = await Repo.InlineUpdateAsync<int, UpdateModel<int>>(
                rq,
                new QuickUpdateConfigs(new[] { "parent", "name", "url", "layout", "status AS enabled=IIF(@Enabled, 0, 200)" })
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