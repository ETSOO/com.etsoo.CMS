using com.etsoo.CMS.RQ.User;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Logined user business logic service interface
    /// 已登录用户业务逻辑服务接口
    /// </summary>
    public interface IUserService : IServiceBase
    {
        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <param name="model">Data model</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordDto model, IPAddress ip);

        /// <summary>
        /// Create user
        /// 创建用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> CreateAsync(UserCreateRQ rq, IPAddress ip);

        /// <summary>
        /// Delete single user
        /// 删除单个用户
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Action result</returns>
        ValueTask<IActionResult> DeleteAsync(string id);

        /// <summary>
        /// Query history user
        /// 查询操作历史用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task HistoryAsync(UserHistoryQueryRQ rq, HttpResponse response);

        /// <summary>
        /// Query user
        /// 查询用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task QueryAsync(UserQueryRQ rq, HttpResponse response);

        /// <summary>
        /// Reset password
        /// 重置密码
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="passphrase">For encription of the password</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        ValueTask<ActionResult> ResetPasswordAsync(string id, string passphrase, IPAddress ip);

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        /// <param name="deviceId">Device id</param>
        /// <returns>Task</returns>
        Task SignoutAsync(string deviceId);

        /// <summary>
        /// Update user
        /// 更新用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> UpdateAsync(UserUpdateRQ rq, IPAddress ip);

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task UpdateReadAsync(string id, HttpResponse response);
    }
}
