using com.etsoo.CMS.Models;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// <summary>
    /// Authorization service interface
    /// 授权服务接口
    /// </summary>
    /// </summary>
    public interface IAuthService : ICommonService
    {
        ValueTask<(IActionResult Result, string? RefreshToken)> LoginAsync(LoginDto data, CancellationToken cancellationToken = default);

        ValueTask<(IActionResult, string?)> RefreshTokenAsync(string token, RefreshTokenDto model, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier);
    }
}
