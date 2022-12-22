using com.etsoo.CMS.Models;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Services;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// <summary>
    /// Authorization service interface
    /// 授权服务接口
    /// </summary>
    /// </summary>
    public interface IAuthService : IServiceBase
    {
        /// <summary>
        /// User login
        /// 用户登录
        /// </summary>
        /// <param name="data">Login data</param>
        /// <returns>Result</returns>
        ValueTask<(IActionResult Result, string? RefreshToken)> LoginAsync(LoginDto data);

        /// <summary>
        /// Refresh token (Related with Login, make sure the logic is consistent)
        /// 刷新令牌 (和登录相关，确保逻辑一致)
        /// </summary>
        /// <param name="token">Refresh token</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        ValueTask<(IActionResult, string?)> RefreshTokenAsync(string token, RefreshTokenDto model);

        /// <summary>
        /// Web init call
        /// Web初始化调用
        /// </summary>
        /// <param name="rq">Rquest data</param>
        /// <param name="identifier">User identifier</param>
        /// <returns>Result</returns>
        ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier);
    }
}
