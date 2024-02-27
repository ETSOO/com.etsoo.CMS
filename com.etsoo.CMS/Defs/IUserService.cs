using com.etsoo.CMS.RQ.User;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Logined user business logic service interface
    /// 已登录用户业务逻辑服务接口
    /// </summary>
    public interface IUserService : ICommonService
    {
        ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordDto model, CancellationToken cancellationToken = default);

        Task<IActionResult> CreateAsync(UserCreateRQ rq, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken = default);

        Task HistoryAsync(UserHistoryQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default);

        Task QueryAsync(UserQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default);

        ValueTask<ActionResult> ResetPasswordAsync(string id, string passphrase, CancellationToken cancellationToken = default);

        Task SignoutAsync(string deviceId, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdateAsync(UserUpdateRQ rq, CancellationToken cancellationToken = default);

        Task UpdateReadAsync(string id, HttpResponse response, CancellationToken cancellationToken = default);
    }
}
