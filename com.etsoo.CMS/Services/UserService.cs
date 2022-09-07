using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.User;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.Services;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Logined user business logic service
    /// 已登录用户业务逻辑服务
    /// </summary>
    public class UserService : SqliteService<UserRepo>
    {
        protected readonly IServiceUser user;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <param name="logger">Logger</param>
        public UserService(IMyApp app, IServiceUser user, ILogger logger)
            : base(app, new UserRepo(app, user), logger)
        {
            this.user = user;
        }

        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <param name="model">Data model</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordDto model, IPAddress ip)
        {
            // Current user
            var user = await Repo.GetCurrentUserAsync();

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
            await Repo.UpdatePasswordAsync(password);

            // Add audit
            await Repo.AddAuditAsync(AuditKind.ChangePassword, user.Id, "Change self password", null, ip);

            // Return
            return ActionResult.Success;
        }

        /// <summary>
        /// Create user
        /// 创建用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(UserCreateRQ rq, IPAddress ip)
        {
            var result = await Repo.CreateAsync(rq);
            await Repo.AddAuditAsync(AuditKind.CreateUser, rq.Id, $"Create user {rq.Id}", ip, result, rq);
            return result;
        }

        /// <summary>
        /// Delete single user
        /// 删除单个用户
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Action result</returns>
        public virtual async ValueTask<ActionResult> DeleteAsync(string id)
        {
            return await Repo.DeleteAsync(id);
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
            await Repo.HistoryAsync(rq, response);
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
            await Repo.QueryAsync(rq, response);
        }

        /// <summary>
        /// Reset password
        /// 重置密码
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="passphrase">For encription of the password</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        public async ValueTask<ActionResult> ResetPasswordAsync(string id, string passphrase, IPAddress ip)
        {
            // Forbid reset current user's password
            if (user.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) return ApplicationErrors.NoValidData.AsResult("id");

            // New password
            var password = CryptographyUtils.CreateRandString(RandStringKind.DigitAndLetter, 6).ToString();

            // Hash password
            var passwordHashed = await App.HashPasswordAsync(id + password);

            // Update
            await Repo.ResetPasswordAsync(id, passwordHashed);

            // Return with encription
            var result = ActionResult.Success;
            result.Data["password"] = EncryptWeb(password, passphrase);

            // Log
            await Repo.AddAuditAsync(AuditKind.ResetUserPassword, id, $"Reset user {id} password", ip, result);

            return result;
        }

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        /// <param name="deviceId">Device id</param>
        /// <returns>Task</returns>
        public async Task SignoutAsync(string deviceId)
        {
            await Repo.SignoutAsync(deviceId);
        }

        /// <summary>
        /// Update user
        /// 更新用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(UserUpdateRQ rq, IPAddress ip)
        {
            var (result, _) = await Repo.InlineUpdateAsync(rq, new QuickUpdateConfigs(new[] { "role", "status AS enabled=IIF(@Enabled, 0, 200)" })
            {
                TableName = "users",
                IdField = "id"
            });
            await Repo.AddAuditAsync(AuditKind.UpdateUser, rq.Id, $"Update user {rq.Id}", ip, result, rq);
            return result;
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(string id, HttpResponse response)
        {
            await Repo.UpdateReadAsync(response, id);
        }
    }
}
