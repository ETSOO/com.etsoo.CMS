using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using System.Reflection;
using System.Text.Json;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Website repository
    /// 网站仓库
    /// </summary>
    public class WebsiteRepo : CommonRepo
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">User</param>
        public WebsiteRepo(IMyApp app, IServiceUser? user)
            : base(app, "website", user)
        {

        }

        /// <summary>
        /// Create settings
        /// 创建设置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateSettingsAsync(WebsiteUpdateSettingsRQ rq)
        {
            if (string.IsNullOrEmpty(rq.Domain))
            {
                return ApplicationErrors.NoValidData.AsResult("domain");
            }

            if (string.IsNullOrEmpty(rq.Title))
            {
                return ApplicationErrors.NoValidData.AsResult("title");
            }

            var parameters = FormatParameters(rq);
            var command = CreateCommand($"INSERT INTO website(domain, title, description, keywords) VALUES (@{nameof(rq.Domain)}, @{nameof(rq.Title)}, @{nameof(rq.Description)}, @{nameof(rq.Keywords)})", parameters);
            await ExecuteAsync(command);

            return ActionResult.Success;
        }

        /// <summary>
        /// Create or update resource
        /// 创建或更新资源
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Action result</returns>
        public async Task<IActionResult> CreateOrUpdateResourceAsync(ResourceCreateRQ model)
        {
            var parameters = FormatParameters(model);

            AddSystemParameters(parameters);

            var command = CreateCommand($@"INSERT OR REPLACE INTO resources (id, value) VALUES (@{nameof(model.Id)}, @{nameof(model.Value)})", parameters);

            await ExecuteAsync(command);

            return ActionResult.Success;
        }

        /// <summary>
        /// Create service
        /// 创建服务
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> CreateServiceAsync(ServiceCreateRQ model)
        {
            var parameters = FormatParameters(model);

            AddSystemParameters(parameters);

            var command = CreateCommand($@"INSERT INTO services (id, app, secret, status, refreshTime)
                VALUES (@{nameof(model.Id)}, @{nameof(model.App)}, @{nameof(model.Secret)}, IIF(@{nameof(model.Enabled)}, 0, 200), DATETIME('now'))", parameters);

            await ExecuteAsync(command);

            return ActionResult.Success;
        }

        /// <summary>
        /// Dashboard data
        /// 仪表盘数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task DashboardAsync(HttpResponse response)
        {
            var parameters = new DbParameters();

            AddSystemParameters(parameters);

            var site = $"domain, version".ToJsonCommand(true);
            var article = $"id, title, refreshTime, author".ToJsonCommand();
            var audits = $"rowid, title, creation, author".ToJsonCommand();
            var command = CreateCommand(@$"SELECT {site} FROM website;
                SELECT {article} FROM (SELECT * FROM articles ORDER BY refreshTime DESC LIMIT 6);
                SELECT {audits} FROM (SELECT rowid, * FROM audits WHERE author = {SysUserField} AND kind <> {(byte)AuditKind.TokenLogin} ORDER BY rowid DESC LIMIT 4)", parameters);

            await ReadJsonToStreamAsync(command, response, new[] { "site", "articles", "audits" });
        }

        /// <summary>
        /// Initialize website
        /// 初始化网站
        /// </summary>
        /// <param name="rq">Reqeust data</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> InitializeAsync(InitializeRQ rq)
        {
            var parameters = FormatParameters(rq);
            var command = CreateCommand($"INSERT INTO website (domain, title) SELECT @{nameof(rq.Domain)}, @{nameof(rq.Title)} WHERE NOT EXISTS (SELECT * FROM website)", parameters);
            await ExecuteAsync(command);
            return ActionResult.Success;
        }

        /// <summary>
        /// Read JSON data
        /// 读取 JSON 数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task ReadJsonDataAsync(HttpResponse response)
        {
            var json = $"json(jsonData) AS jsonData".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM website LIMIT 1");

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Read JSON data to object
        /// 读取 JSON 数据到对象
        /// </summary>
        /// <typeparam name="T">Generic object type</typeparam>
        /// <returns>Result</returns>
        public async Task<T?> ReadJsonDataAsync<T>()
        {
            var command = CreateCommand($"SELECT jsonData FROM website LIMIT 1");
            var raw = await ExecuteScalarAsync<string?>(command);
            if (string.IsNullOrEmpty(raw)) return default;
            else return JsonSerializer.Deserialize<T>(raw, SharedUtils.JsonDefaultSerializerOptions);
        }

        /// <summary>
        /// Read service
        /// 读取服务
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        public async Task<DbService?> ReadServiceAsync(string id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);
            var command = CreateCommand($"SELECT app, secret FROM services WHERE id = @{nameof(id)} AND status < 200", parameters);
            return await QueryAsAsync<DbService>(command);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="rootUrl">Root URL</param>
        /// <returns>Task</returns>
        public async Task ReadSettingsAsync(HttpResponse response, string? rootUrl)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(rootUrl), rootUrl);

            var json = $"domain, title, keywords, description, @{nameof(rootUrl)} AS rootUrl, json(jsonData) AS jsonData".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM website LIMIT 1", parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <returns>Result</returns>
        public async Task<DbWebsite?> ReadSettingsAsync()
        {
            var command = CreateCommand($"SELECT rowid, domain, title, description, keywords, jsonData FROM website LIMIT 1");
            return await QueryAsAsync<DbWebsite>(command);
        }

        /// <summary>
        /// Query plugin services
        /// 查询插件服务
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryServicesAsync(HttpResponse response)
        {
            var json = $"id, app, refreshTime, {"status < 200".ToJsonBool()} AS enabled".ToJsonCommand();
            var command = CreateCommand($"SELECT {json} FROM services");

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Query available tabs
        /// 查询可用栏目
        /// </summary>
        /// <returns></returns>
        public async Task<TabLink[]> QueryTabsAsync()
        {
            var command = CreateCommand(@$"SELECT id, name, layout, url FROM tabs WHERE status < 200");
            return await QueryAsListAsync<TabLink>(command);
        }

        /// <summary>
        /// Query resources
        /// 查询资源
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryResourcesAsync(HttpResponse response)
        {
            var json = $"id, value".ToJsonCommand();
            var command = CreateCommand($"SELECT {json} FROM resources");

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Update resource URL
        /// 更新资源路径
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        public async Task UpdateResurceUrlAsync(string oldResourceUrl, string resouceUrl)
        {
            var oldField = nameof(oldResourceUrl);
            var newField = nameof(resouceUrl);

            var parameters = new DbParameters();
            parameters.Add(oldField, oldResourceUrl);
            parameters.Add(newField, resouceUrl);

            var command = CreateCommand($"""
                UPDATE tabs SET logo = REPLACE(logo, @{oldField}, @{newField});
                UPDATE articles SET logo = REPLACE(logo, @{oldField}, @{newField}), content = REPLACE(content, @{oldField}, @{newField}), slideshow = REPLACE(slideshow, @{oldField}, @{newField});
                """, parameters);

            await ExecuteAsync(command);
        }

        /// <summary>
        /// Upgrade system
        /// 升级系统
        /// </summary>
        /// <returns>Task</returns>
        public async Task<IActionResult> UpgradeSystemAsync()
        {
            // New version
            var newVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            if (newVersion == null)
            {
                return ApplicationErrors.NoValidData.AsResult("version");
            }

            // Current version
            // When null means initialization
            var versionText = await ExecuteScalarAsync<string>(CreateCommand("SELECT version FROM website"));
            if (!string.IsNullOrEmpty(versionText))
            {
                var version = new Version(versionText);
                var result = version.CompareTo(newVersion);

                // Same versions
                if (result >= 0)
                {
                    return ActionResult.Success;
                }

                // Actions
                if (version.CompareTo(new Version("1.0.2")) < 0)
                {
                    var command102 = CreateCommand($@"
                    ALTER TABLE website ADD COLUMN jsonData TEXT;

                    ALTER TABLE services ADD COLUMN jsonData TEXT;

                    ALTER TABLE tabs ADD COLUMN logo TEXT;
                    ALTER TABLE tabs ADD COLUMN description TEXT;
                    ALTER TABLE tabs ADD COLUMN jsonData TEXT;

                    ALTER TABLE articles ADD COLUMN jsonData TEXT;
                ");

                    await ExecuteAsync(command102);
                }

                if (version.CompareTo(new Version("1.0.3")) < 0)
                {
                    var command102 = CreateCommand($@"
                    ALTER TABLE tabs ADD COLUMN icon TEXT;
                ");

                    await ExecuteAsync(command102);
                }
            }

            // Update version
            var updateParameters = new DbParameters();
            updateParameters.Add(nameof(newVersion), newVersion.ToString(3));
            var command = CreateCommand($"UPDATE website SET version = @{nameof(newVersion)}", updateParameters);
            await QueryAsAsync<DbWebsite>(command);

            return ActionResult.Success;
        }
    }
}
