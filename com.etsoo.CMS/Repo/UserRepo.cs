using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CoreFramework.User;
using Dapper;

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
        /// Get current user data
        /// 获取当前用户数据
        /// </summary>
        /// <returns>Result</returns>
        public async Task<DbUser?> GetCurrentUserAsync()
        {
            var parameters = new DynamicParameters();

            AddSystemParameters(parameters);

            var command = CreateCommand($"SELECT id, password, role, status, frozenTime FROM users WHERE id = {SysUserField}", parameters);

            return await QueryAsAsync<DbUser>(command);
        }

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        /// <param name="device">Device id</param>
        /// <returns>Task</returns>
        public async Task SignoutAsync(string device)
        {
            var parameters = new DynamicParameters();
            parameters.Add("device", device);

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM devices WHERE user = {SysUserField} AND device = @device", parameters);

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
            var parameters = new DynamicParameters();
            parameters.Add("password", password);

            AddSystemParameters(parameters);

            var command = CreateCommand($"UPDATE users SET password = @password WHERE id = {SysUserField}", parameters);

            await ExecuteAsync(command);
        }
    }
}
