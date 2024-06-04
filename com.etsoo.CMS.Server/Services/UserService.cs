using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.User;
using com.etsoo.CMS.Server;
using com.etsoo.CMS.Server.Services;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Logined user business logic service
    /// 已登录用户业务逻辑服务
    /// </summary>
    public class UserService : CommonUserService, IUserService
    {
        readonly IPAddress ip;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        public UserService(IMyApp app, IMyUserAccessor userAccessor, ILogger<UserService> logger)
            : base(app, userAccessor.UserSafe, "user", logger)
        {
            ip = userAccessor.Ip;
        }

        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <param name="model">Data model</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordDto model, CancellationToken cancellationToken = default)
        {
            // Current user
            var user = await GetCurrentUserAsync(cancellationToken);

            // User check
            if (!ServiceUtils.CheckUser(user, out var checkResult))
            {
                return checkResult;
            }

            // Password match
            // Match in code rather than in SP to prevent logic cheat
            if (model.OldPassword == null
                || !user.Password.Equals(await App.HashPasswordAsync(user.Id + model.OldPassword)))
            {
                return ApplicationErrors.NoPasswordMatch.AsResult();
            }

            // Hash password
            var password = await App.HashPasswordAsync(user.Id + model.Password);

            // Update password
            await UpdatePasswordAsync(password, cancellationToken);

            // Add audit
            var auditTitle = Resources.ChangeSelfPassword;
            await AddAuditAsync<string?>(AuditKind.ChangePassword, user.Id, auditTitle, null, null, ip, cancellationToken: cancellationToken);

            // Return
            return ActionResult.Success;
        }

        /// <summary>
        /// Create user
        /// 创建用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(UserCreateRQ rq, CancellationToken cancellationToken = default)
        {
            rq.Id = rq.Id.ToLower();

            // Hash password
            var passwordHashed = await App.HashPasswordAsync(rq.Id + rq.Password);
            rq.Password = passwordHashed;

            /*
            var parameters = FormatParameters(rq);

            var command = CreateCommand($@"INSERT INTO users (id, password, role, status, creation)
                VALUES (@{nameof(rq.Id)}, '', @{nameof(rq.Role)}, IIF(@{nameof(rq.Enabled)}, 0, 200), DATETIME('now', 'utc'))", parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);
            */

            var id = await SqlInsertAsync<UserCreateRQ, string>(rq, cancellationToken: cancellationToken);

            var result = string.IsNullOrEmpty(id) ? ApplicationErrors.ItemExists.AsResult() : ActionResult.Success;

            var auditTitle = Resources.CreateUser.Replace("{0}", rq.Id);
            await AddAuditAsync(AuditKind.CreateUser, rq.Id, auditTitle, ip, result, rq, MyJsonSerializerContext.Default.UserCreateRQ, cancellationToken);

            return result;
        }

        /// <summary>
        /// Delete single user
        /// 删除单个用户
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));

            var command = CreateCommand($"DELETE FROM users WHERE id = @{nameof(id)} AND refreshTime IS NULL", parameters, cancellationToken: cancellationToken);

            var affected = await ExecuteAsync(command);

            var result = affected > 0 ? ActionResult.Success : ApplicationErrors.NoId.AsResult();

            var auditTitle = Resources.DeleteUser.Replace("{0}", id);
            await AddAuditAsync(AuditKind.DeleteUser, id, auditTitle, ip, result, id, null, cancellationToken: cancellationToken);

            return result;
        }

        /// <summary>
        /// Get current user data
        /// 获取当前用户数据
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async Task<DbUser?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();

            AddSystemParameters(parameters);

            var command = CreateCommand($"SELECT id, password, role, status, frozenTime FROM users WHERE id = {SysUserField}", parameters, cancellationToken: cancellationToken);

            return await QueryAsAsync<DbUser>(command);
        }

        /// <summary>
        /// Query user history
        /// 查询用户操作历史
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public Task HistoryAsync(UserHistoryQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            /*
            var parameters = FormatParameters(rq);

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
            var command = CreateCommand($"SELECT {json} FROM (SELECT {"rowid AS " + fields} FROM audits {conditions} {rq.GetOrderCommand()} {limit})", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
            */

            return SqlSelectJsonAsync(rq, ["rowid AS id", "kind", "title", "content", "creation", "ip", "flag"], response, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query user
        /// 查询用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(UserQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            /*
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var fields = "id, role, status, creation, refreshTime";
            var json = $"{fields}, {$"id = {SysUserField}".ToJsonBool()} AS isSelf".ToJsonCommand();

            var items = new List<string>();
            if (!string.IsNullOrEmpty(rq.Sid)) items.Add($"id = @{nameof(rq.Sid)}");
            if (rq.Role is not null) items.Add($"role = @{nameof(rq.Role)}");
            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.QueryPaging);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand($"SELECT {json} FROM (SELECT {fields} FROM users {conditions} {rq.QueryPaging.GetOrderCommand()} {limit})", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
            */

            await SqlSelectJsonAsync(rq, ["id", "role", "status", "creation", "refreshTime", $"IIF(id = {SysUserField}, TRUE, FALSE):boolean AS isSelf"], response, true, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Reset password
        /// 重置密码
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="passphrase">For encription of the password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask<ActionResult> ResetPasswordAsync(string id, string passphrase, CancellationToken cancellationToken = default)
        {
            // Forbid reset current user's password
            if (User?.Id.Equals(id, StringComparison.OrdinalIgnoreCase) == true) return ApplicationErrors.NoValidData.AsResult("id");

            // New password
            var password = CryptographyUtils.CreateRandString(RandStringKind.DigitAndLetter, 6).ToString();

            // Hash password
            var passwordHashed = await App.HashPasswordAsync(id + password);

            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));

            var passwordParam = passwordHashed.ToDbString(true, 256);
            parameters.Add(nameof(password), passwordParam);

            var command = CreateCommand($"UPDATE users SET password = @{nameof(password)} WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);

            // Return with encription
            var result = ActionResult.Success;
            result.Data["password"] = EncryptWeb(password, passphrase);

            // Log
            var auditTitle = Resources.ResetUserPassword.Replace("{0}", id);
            await AddAuditAsync<string?>(AuditKind.ResetUserPassword, id, auditTitle, ip, result, cancellationToken: cancellationToken);

            return result;
        }

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        /// <param name="deviceId">Device id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task SignoutAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(deviceId), deviceId);

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM devices WHERE user = {SysUserField} AND device = @{nameof(deviceId)}", parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);
        }

        /// <summary>
        /// Update user
        /// 更新用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(UserUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var (result, _) = await InlineUpdateAsync(rq, new QuickUpdateConfigs(["role", "status AS enabled=IIF(@Enabled, 0, 200)"])
            {
                TableName = "users",
                IdField = "id"
            }, cancellationToken: cancellationToken);
            var auditTitle = Resources.UpdateUser.Replace("{0}", rq.Id);
            await AddAuditAsync(AuditKind.UpdateUser, rq.Id, auditTitle, ip, result, rq, MyJsonSerializerContext.Default.UserUpdateRQ, cancellationToken);
            return result;
        }

        /// <summary>
        /// Update user password
        /// 更新用户密码
        /// </summary>
        /// <param name="password">New password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task UpdatePasswordAsync(string password, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(password), password.ToDbString(true, 256));

            AddSystemParameters(parameters);

            var command = CreateCommand($"UPDATE users SET password = @{nameof(password)} WHERE id = {SysUserField}", parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(string id, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));

            var json = $"id, role, refreshTime, {"status < 200".ToJsonBool()} AS enabled".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM users WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }
    }
}
