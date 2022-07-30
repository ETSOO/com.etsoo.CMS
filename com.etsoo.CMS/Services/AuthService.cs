using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Repo;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Authorization service
    /// 授权服务
    /// </summary>
    public class AuthService : CommonService<AuthRepo>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="logger">Logger</param>
        public AuthService(IMyApp app, ILogger logger)
            : base(app, new AuthRepo(app), logger)
        {
        }

        // Hash password
        private async Task<string> HashPasswordAsync(string id, string password)
        {
            return await App.HashPasswordAsync(id + password);
        }

        public async ValueTask<(IActionResult Result, string? RefreshToken)> LoginAsync(LoginDto data)
        {
            // Hashed password
            var hashedPassword = await HashPasswordAsync(data.Id, data.Pwd);

            // Get user data
            var (user, setup) = await Repo.GetUserAsync(data.Id);
            if (user == null)
            {
                if (setup)
                {
                    try
                    {
                        // Setup
                        await Repo.SetupAsync(data.Id, hashedPassword, data.Ip);

                        // Read the user again
                        (user, setup) = await Repo.GetUserAsync(data.Id);
                    }
                    catch (Exception ex)
                    {
                        return (LogException(ex), null);
                    }
                }

                if (user == null)
                {
                    // User not found error
                    return (ApplicationErrors.NoUserFound.AsResult(), null);
                }
            }

            // Frozen time check first
            if (user.FrozenTime != null && DateTime.UtcNow <= user.FrozenTime)
            {
                var frozenResult = ApplicationErrors.DeviceFrozen.AsResult();
                frozenResult.Data.Add("FrozenTime", user.FrozenTime);
                return (frozenResult, null);
            }

            // Status check
            if (user.Status >= EntityStatus.Inactivated)
            {
                return (ApplicationErrors.AccountDisabled.AsResult(), null);
            }

            // Successful login
            bool success;

            // Password match
            if (!user.Password.Equals(hashedPassword))
            {
                success = false;
            }
            else
            {
                success = true;
            }

            // Add audit
            await AddAudit(user.Id, data.Ip);

            return (null, null);
        }

        /// <summary>
        /// Web init call
        /// Web初始化调用
        /// </summary>
        /// <param name="rq">Rquest data</param>
        /// <param name="identifier">User identifier</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier)
        {
            // Init call
            return await InitCallAsync(rq, identifier);
        }
    }
}
