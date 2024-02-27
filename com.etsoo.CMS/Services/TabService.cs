using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Database;
using com.etsoo.HTTP;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website tab service
    /// 网站栏目业务逻辑服务
    /// </summary>
    public class TabService : CommonService, ITabService
    {
        readonly IPAddress ip;
        readonly IStorage storage;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        public TabService(IMyApp app, IMyUserAccessor userAccessor, ILogger<TabService> logger, IStorage storage)
            : base(app, userAccessor.UserSafe, "tab", logger)
        {
            ip = userAccessor.Ip;
            this.storage = storage;
        }

        /// <summary>
        /// Create tab
        /// 创建栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(TabCreateRQ rq, CancellationToken cancellationToken = default)
        {
            if (rq.Url != "/")
                rq.Url = rq.Url.TrimEnd('/');

            var parameters = FormatParameters(rq);

            var command = CreateCommand(@$"INSERT INTO tabs (parent, name, description, logo, icon, url, layout, orderIndex, status, articles, refreshTime, jsonData)
                VALUES (@{nameof(rq.Parent)}, @{nameof(rq.Name)}, @{nameof(rq.Description)}, @{nameof(rq.Logo)}, @{nameof(rq.Icon)}, @{nameof(rq.Url)}, @{nameof(rq.Layout)}, 0, IIF(@{nameof(rq.Enabled)}, 0, 200), 0, DATETIME('now', 'utc'), @{nameof(rq.JsonData)}); SELECT last_insert_rowid();", parameters, cancellationToken: cancellationToken);

            var tabId = await ExecuteScalarAsync<int>(command);

            var result = new ActionDataResult<int>(ActionResult.Success, tabId);

            var id = result.Data;
            await AddAuditAsync(AuditKind.CreateTab, id.ToString(), $"Create tab {id}", ip, result.Result, rq, MyJsonSerializerContext.Default.TabCreateRQ, cancellationToken);
            return result.MergeData("id");
        }

        /// <summary>
        /// Delete tab
        /// 删除栏目
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var command = CreateCommand($"DELETE FROM tabs WHERE id = @{nameof(id)} AND articles = 0", parameters, cancellationToken: cancellationToken);

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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task ListAsync(TiplistRQ<int> rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = rq.AsParameters(App);

            var fields = "id, name";
            var json = "id, name AS label".ToJsonCommand();

            var items = new List<string>();
            if (rq.Id != null) items.Add($"id = @{nameof(rq.Id)}");
            if (rq.ExcludedIds != null) items.Add($"NOT EXISTS (SELECT * FROM json_each(${nameof(rq.ExcludedIds)}) t WHERE t.value = tabs.id)");
            if (!string.IsNullOrEmpty(rq.Keyword)) items.Add($"name LIKE '%' || @{nameof(rq.Keyword)} || '%'");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.Items ?? 16, 0);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand($"SELECT {json} FROM (SELECT {fields} FROM tabs {conditions} ORDER BY orderIndex ASC, name ASC {limit})", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Query tab
        /// 查询栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(TabQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = FormatParameters(rq);

            var fields = "id, parent, name, url, logo, articles";
            var json = fields.ToJsonCommand();

            var items = new List<string>();
            if (rq.Parent is not null) items.Add($"parent = @{nameof(rq.Parent)}");
            else items.Add($"parent IS NULL");

            if (rq.Enabled is true) items.Add($"status < 200");
            else if (rq.Enabled is false) items.Add($"status >= 200");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.BatchSize, rq.CurrentPage);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand($"SELECT {json} FROM (SELECT {fields} FROM tabs {conditions} {rq.GetOrderCommand()} {limit})", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Read logo
        /// 读取照片
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async Task<string?> ReadLogoAsync(int id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var command = CreateCommand(@$"SELECT IFNULL(logo, '') FROM tabs WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            return await ExecuteScalarAsync<string?>(command);
        }

        /// <summary>
        /// Sort data
        /// 数据排序
        /// </summary>
        /// <param name="sortData">Data to sort</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Rows affected</returns>
        public async Task<int> SortAsync(Dictionary<int, short> sortData, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            var json = await SharedUtils.JsonSerializeAsync(sortData);
            parameters.Add(nameof(json), json);

            var command = CreateCommand($"UPDATE tabs AS t SET orderIndex = s.value FROM json_each(@{nameof(json)}) AS s WHERE t.id = s.key", parameters, cancellationToken: cancellationToken);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// Update tab
        /// 更新栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(TabUpdateRQ rq, CancellationToken cancellationToken = default)
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

            var (result, _) = await InlineUpdateAsync<int, UpdateModel<int>>(
                rq,
                new QuickUpdateConfigs(["parent", "name", "description", "jsonData", "logo", "icon", "url", "layout", "status AS enabled=IIF(@Enabled, 0, 200)"])
                {
                    TableName = "tabs",
                    IdField = "id"
                },
                $"refreshTime = @{nameof(refreshTime)}", parameters, cancellationToken
             );
            await AddAuditAsync(AuditKind.UpdateTab, rq.Id.ToString(), $"Update tab {rq.Id}", ip, result, rq, MyJsonSerializerContext.Default.TabUpdateRQ, cancellationToken);
            return result;
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <param name="logoStream">Logo stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New URL</returns>
        public async ValueTask<string?> UploadLogoAsync(int id, Stream logoStream, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            var logo = await ReadLogoAsync(id, cancellationToken);
            if (logo == null)
            {
                return null;
            }

            // File path
            var path = $"/Resources/TabLogos/t{id.ToString().PadLeft(6, '0')}.{Path.GetRandomFileName()}{extension}";

            // Save the stream to file directly
            var saveResult = await storage.WriteAsync(path, logoStream, WriteCase.CreateNew, cancellationToken);

            if (saveResult)
            {
                // New avatar URL
                var url = storage.GetUrl(path);

                // Repo update
                if (await UpdateLogoAsync(id, url, cancellationToken) > 0)
                {
                    // Audit
                    await AddAuditAsync(AuditKind.UpdateTabLogo, id.ToString(), $"Update tab {id} logo", new Dictionary<string, object> { ["Logo"] = logo, ["NewLogo"] = url }, null, ip, cancellationToken: cancellationToken);

                    // Return
                    return url;
                }
            }

            Logger.LogError("Logo write path is {path}", path);

            await AddAuditAsync(AuditKind.UpdateTabLogo, id.ToString(), $"Update tab {id} logo", ip, new ActionResult(), path, null, cancellationToken);

            return null;
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <param name="url">Photo URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async Task<int> UpdateLogoAsync(int id, string url, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);
            parameters.Add(nameof(url), url);

            var command = CreateCommand(@$"UPDATE tabs SET logo = @{nameof(url)} WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var json = $"id, parent, name, logo, icon, description, jsonData, url, layout, articles, {"status < 200".ToJsonBool()} AS enabled".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM tabs WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// View tab ancestors
        /// 浏览上层栏目
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ParentTab[]> AncestorReadAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            var sql = $"""
                WITH ctx(id, parent, name, url, layout, level) AS (
                    SELECT f.id, f.parent, f.name, f.url, f.layout, 0 FROM tabs AS f WHERE f.id IN ({string.Join(',', ids)}) AND f.status < 200
                        UNION ALL
                    SELECT t.id, t.parent, t.name, t.url, t.layout, ctx.level + 1 FROM tabs AS t INNER JOIN ctx ON t.id = ctx.parent WHERE ctx.parent IS NOT NULL AND t.status < 200
                ) SELECT * FROM ctx
                """;
            var command = CreateCommand(sql, cancellationToken: cancellationToken);
            return await QueryAsListAsync<ParentTab>(command);
        }

        /// <summary>
        /// Read for ancestors
        /// 上层栏目浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AncestorReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var command = CreateCommand($"WITH ctx(id) AS (SELECT @{nameof(id)} UNION ALL SELECT t.parent FROM tabs AS t INNER JOIN ctx ON t.id = ctx.id WHERE t.parent IS NOT NULL) SELECT json_group_array(id) FROM ctx", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }
    }
}