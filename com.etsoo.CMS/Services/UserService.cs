using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Logined user business logic service
    /// 已登录用户业务逻辑服务
    /// </summary>
    public class UserService : SqliteService<UserRepo>
    {
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
            await Repo.AddAuditAsync(AuditKind.ChangePassword, user.Id, "Change password", null, ip);

            // Return
            return ActionResult.Success;
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
    }
}
