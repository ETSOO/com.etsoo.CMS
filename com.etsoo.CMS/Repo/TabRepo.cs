using com.etsoo.CMS.Application;
using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Website tab repository
    /// 网站栏目仓库
    /// </summary>
    public class TabRepo : CommonRepo
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">User</param>
        public TabRepo(IMyApp app, IServiceUser? user)
            : base(app, "tab", user)
        {

        }

        /// <summary>
        /// Create tab
        /// 创建栏目
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Action result</returns>
        public async Task<ActionDataResult<int>> CreateAsync(TabCreateRQ model)
        {
            model.Url = model.Url.TrimEnd('/');

            var parameters = FormatParameters(model);

            AddSystemParameters(parameters);

            var command = CreateCommand(@$"INSERT INTO tabs (parent, name, url, layout, orderIndex, status, articles, refreshTime)
                VALUES (@{nameof(model.Parent)}, @{nameof(model.Name)}, @{nameof(model.Url)}, @{nameof(model.Layout)}, 0, IIF(@{nameof(model.Enabled)}, 0, 200), 0, DATETIME('now')); SELECT last_insert_rowid();", parameters);

            var tabId = await ExecuteScalarAsync<int>(command);

            return new ActionDataResult<int>(ActionResult.Success, tabId);
        }


        /// <summary>
        /// Delete single tab
        /// 删除单个栏目
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Action result</returns>
        public async ValueTask<ActionResult> DeleteAsync(int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM tabs WHERE id = @{nameof(id)} AND articles = 0", parameters);

            var result = await ExecuteAsync(command);

            if (result > 0)
                return ActionResult.Success;
            else
                return ApplicationErrors.NoId.AsResult();
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
            var parameters = rq.AsParameters(App);

            AddSystemParameters(parameters);

            var fields = "id, name";
            var json = "id, name AS label".ToJsonCommand();

            var items = new List<string>();
            if (rq.Id != null) items.Add($"id = @{nameof(rq.Id)}");
            if (rq.ExcludedIds != null) items.Add($"NOT EXISTS (SELECT * FROM json_each(${nameof(rq.ExcludedIds)}) t WHERE t.value = tabs.id)");
            if (!string.IsNullOrEmpty(rq.Keyword)) items.Add($"name LIKE '%' || @{nameof(rq.Keyword)} || '%'");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.Items ?? 16, 0);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand($"SELECT {json} FROM (SELECT {fields} FROM tabs {conditions} ORDER BY orderIndex ASC, name ASC {limit})", parameters);

            await ReadJsonToStreamAsync(command, response);
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
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var fields = "id, parent, name, url, articles";
            var json = fields.ToJsonCommand();

            var items = new List<string>();
            if (rq.Parent is not null) items.Add($"parent = @{nameof(rq.Parent)}");
            else items.Add($"parent IS NULL");

            if (rq.Enabled is true) items.Add($"status < 200");
            else if (rq.Enabled is false) items.Add($"status >= 200");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.BatchSize, rq.CurrentPage);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand($"SELECT {json} FROM (SELECT {fields} FROM tabs {conditions} {rq.GetOrderCommand()} {limit})", parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Sort data
        /// 数据排序
        /// </summary>
        /// <param name="sortData">Data to sort</param>
        /// <returns>Rows affected</returns>
        public async Task<int> SortAsync(Dictionary<int, short> sortData)
        {
            var parameters = new DbParameters();
            var json = await SharedUtils.JsonSerializeAsync(sortData);
            parameters.Add(nameof(json), json);

            AddSystemParameters(parameters);

            var command = CreateCommand($"UPDATE tabs AS t SET orderIndex = s.value FROM json_each(@{nameof(json)}) AS s WHERE t.id = s.key", parameters);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// View tab update JSON data to HTTP Response
        /// 浏览栏目更新JSON数据到HTTP响应
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(HttpResponse response, int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var json = $"id, parent, name, url, layout, articles, {"status < 200".ToJsonBool()} AS enabled".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM tabs WHERE id = @{nameof(id)}", parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// View tab ancestors JSON data to HTTP Response
        /// 浏览上层栏目JSON数据到HTTP响应
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        public async Task AncestorReadAsync(HttpResponse response, int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var command = CreateCommand($"WITH ctx(id) AS (SELECT @{nameof(id)} UNION ALL SELECT t.parent FROM tabs AS t INNER JOIN ctx ON t.id = ctx.id WHERE t.parent IS NOT NULL) SELECT json_group_array(id) FROM ctx", parameters);

            await ReadJsonToStreamAsync(command, response);
        }
    }
}
