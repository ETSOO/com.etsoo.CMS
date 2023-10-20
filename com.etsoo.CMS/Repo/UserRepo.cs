using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.User;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Logined user repository
    /// 已登录用户仓库
    /// </summary>
    public class UserRepo : CommonRepo
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">User</param>
        public UserRepo(IMyApp app, IServiceUser? user)
            : base(app, "user", user)
        {

        }

        /// <summary>
        /// Create user
        /// 创建用户
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> CreateAsync(UserCreateRQ model)
        {
            var parameters = FormatParameters(model);

            AddSystemParameters(parameters);

            var command = CreateCommand($@"INSERT INTO users (id, password, role, status, creation)
                VALUES (@{nameof(model.Id)}, '', @{nameof(model.Role)}, IIF(@{nameof(model.Enabled)}, 0, 200), DATETIME('now', 'utc'))", parameters);

            await ExecuteAsync(command);

            return ActionResult.Success;
        }

        /// <summary>
        /// Delete single user
        /// 删除单个用户
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(string id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM users WHERE id = @{nameof(id)} AND refreshTime IS NULL", parameters);

            var result = await ExecuteAsync(command);

            if (result > 0)
                return ActionResult.Success;
            else
                return ApplicationErrors.NoId.AsResult();
        }

        /// <summary>
        /// Get current user data
        /// 获取当前用户数据
        /// </summary>
        /// <returns>Result</returns>
        public async Task<DbUser?> GetCurrentUserAsync()
        {
            var parameters = new DbParameters();

            AddSystemParameters(parameters);

            var command = CreateCommand($"SELECT id, password, role, status, frozenTime FROM users WHERE id = {SysUserField}", parameters);

            return await QueryAsAsync<DbUser>(command);
        }

        /// <summary>
        /// Query history user
        /// 查询操作历史用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task HistoryAsync(UserHistoryQueryRQ rq, HttpResponse response)
        {
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var fields = "id, kind, title, content, creation, ip, flag";
            var json = fields.ToJsonCommand();

            var items = new List<string>
            {
                $"author = @{nameof(rq.Author)}"
            };
            if (rq.Kind != null) items.Add($"kind = @{nameof(rq.Kind)}");
            if (rq.CreationStart != null) items.Add($"creation >= @{nameof(rq.CreationStart)}");
            if (rq.CreationEnd != null) items.Add($"creation < @{nameof(rq.CreationEnd)}");
            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.BatchSize, rq.CurrentPage);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand($"SELECT {json} FROM (SELECT {"rowid AS " + fields} FROM audits {conditions} {rq.GetOrderCommand()} {limit})", parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Query user
        /// 查询用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(UserQueryRQ rq, HttpResponse response)
        {
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var fields = "id, role, status, creation, refreshTime";
            var json = $"{fields}, {$"id = {SysUserField}".ToJsonBool()} AS isSelf".ToJsonCommand();

            var items = new List<string>();
            if (!string.IsNullOrEmpty(rq.Sid)) items.Add($"id = @{nameof(rq.Sid)}");
            if (rq.Role is not null) items.Add($"role = @{nameof(rq.Role)}");
            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.BatchSize, rq.CurrentPage);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand($"SELECT {json} FROM (SELECT {fields} FROM users {conditions} {rq.GetOrderCommand()} {limit})", parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        /// <param name="device">Device id</param>
        /// <returns>Task</returns>
        public async Task SignoutAsync(string device)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(device), device);

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM devices WHERE user = {SysUserField} AND device = @{nameof(device)}", parameters);

            await ExecuteAsync(command);
        }

        /// <summary>
        /// Reset password
        /// 重置密码
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="hashedPassword">Hashed password</param>
        /// <returns>Task</returns>
        public async Task ResetPasswordAsync(string id, string hashedPassword)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));

            var password = hashedPassword.ToDbString(true, 256);
            parameters.Add(nameof(password), password);

            var command = CreateCommand($"UPDATE users SET password = @{nameof(password)} WHERE id = @{nameof(id)}", parameters);

            await ExecuteAsync(command);
        }

        /// <summary>
        /// Update user password
        /// 更新用户密码
        /// </summary>
        /// <param name="password">New password</param>
        /// <returns>Task</returns>
        public async Task UpdatePasswordAsync(string password)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(password), password.ToDbString(true, 256));

            AddSystemParameters(parameters);

            var command = CreateCommand($"UPDATE users SET password = @{nameof(password)} WHERE id = {SysUserField}", parameters);

            await ExecuteAsync(command);
        }

        /// <summary>
        /// View update user JSON data to HTTP Response
        /// 浏览更新用户JSON数据到HTTP响应
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(HttpResponse response, string id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));

            AddSystemParameters(parameters);

            var json = $"id, role, refreshTime, {"status < 200".ToJsonBool()} AS enabled".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM users WHERE id = @{nameof(id)}", parameters);

            await ReadJsonToStreamAsync(command, response);
        }
    }
}
