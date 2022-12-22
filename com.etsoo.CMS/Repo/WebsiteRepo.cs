using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using System.Reflection;

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
        /// <returns>Task</returns>
        public async Task ReadSettingsAsync(HttpResponse response)
        {
            var json = $"domain, title, keywords, description".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM website LIMIT 1");

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <returns>Result</returns>
        public async Task<DbWebsite?> ReadSettingsAsync()
        {
            var command = CreateCommand($"SELECT rowid, domain, title, description, keywords FROM website LIMIT 1");
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
        /// Upgrade system
        /// 升级系统
        /// </summary>
        /// <returns>Task</returns>
        public async Task<IActionResult> UpgradeSystemAsync()
        {
            // New version
            var newVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3);
            if (string.IsNullOrEmpty(newVersion))
            {
                return ApplicationErrors.NoValidData.AsResult("version");
            }

            // Current version
            var version = await ExecuteScalarAsync<string>(CreateCommand("SELECT version FROM website"));

            // Same versions
            if (newVersion.Equals(version))
            {
                return ActionResult.Success;
            }

            // Actions
            var parameters = new DbParameters();

            AddSystemParameters(parameters);

            // Update version
            var updateParameters = new DbParameters();
            updateParameters.Add(nameof(newVersion), newVersion);
            var command = CreateCommand($"UPDATE website SET version = @{nameof(newVersion)}", updateParameters);
            await QueryAsAsync<DbWebsite>(command);

            return ActionResult.Success;
        }
    }
}
