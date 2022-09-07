using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;

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
        /// Create service
        /// 创建服务
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Action result</returns>
        public async ValueTask<ActionResult> CreateServiceAsync(ServiceCreateRQ model)
        {
            var parameters = FormatParameters(model);

            AddSystemParameters(parameters);

            var command = CreateCommand($@"INSERT INTO services (id, app, secret, status, refreshTime)
                VALUES (@{nameof(model.Id)}, @{nameof(model.App)}, @{nameof(model.Secret)}, IIF(@{nameof(model.Enabled)}, 0, 200), DATETIME('now'))", parameters);

            await ExecuteAsync(command);

            return ActionResult.Success;
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

            await ReadJsonToStreamAsync(command, response, false);
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

            await ReadJsonToStreamAsync(command, response, false);
        }
    }
}
