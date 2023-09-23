using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Repo;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using System.Globalization;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Authorization service
    /// 授权服务
    /// </summary>
    public class AuthService : CommonService<AuthRepo>, IAuthService
    {
        readonly CoreFramework.Authentication.IAuthService _authService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="logger">Logger</param>
        public AuthService(IMyApp app, ILogger<AuthService> logger)
            : base(app, new AuthRepo(app), logger)
        {
            if (app.AuthService == null) throw new NullReferenceException(nameof(app.AuthService));
            _authService = app.AuthService;
        }

        // Hash password
        private async Task<string> HashPasswordAsync(string id, string password)
        {
            return await App.HashPasswordAsync(id + password);
        }

        private async Task<string> FormatLoginResultAsync(IActionResult result, IServiceUser user, string device)
        {
            // Expiry seconds
            result.Data[Constants.SecondsName] = _authService.AccessTokenMinutes * 60;

            // Role
            result.Data["Role"] = user.RoleValue;

            // Name
            result.Data["Name"] = StringUtils.HideData(user.Id);

            // Refresh token
            var token = new RefreshToken(user.Id, user.Organization, user.ClientIp, user.Region, user.DeviceId, null);

            // Access token
            result.Data[Constants.TokenName] = _authService.CreateAccessToken(user);

            // Refresh token
            var refreshToken = _authService.CreateRefreshToken(token);

            // Hash token
            var hashedToken = await App.HashPasswordAsync(refreshToken);

            // Update
            await Repo.UpdateTokenAsync(user.Id, device, hashedToken);

            // Return
            return refreshToken;
        }

        /// <summary>
        /// User login
        /// 用户登录
        /// </summary>
        /// <param name="data">Login data</param>
        /// <returns>Result</returns>
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
            }

            // User check
            if (!ServiceUtils.CheckUser(user, out var checkResult))
            {
                return (checkResult, null);
            }

            // Successful login
            bool success;

            // Password match
            if (!user.Password.Equals(hashedPassword))
            {
                success = false;

                await Repo.AddLoginFailureAsync(user.Id, user.Failure);
            }
            else
            {
                success = true;
            }

            // Add audit
            await Repo.AddAuditAsync(AuditKind.Login, user.Id, success ? "Login" : "Login Failed", new { data.Device, Success = success }, data.Ip, success ? AuditFlag.Normal : AuditFlag.Warning);

            if (success)
            {
                // Current culture
                var ci = CultureInfo.CurrentCulture;

                // Create token user from result data
                var token = new ServiceUser(user.Role, user.Id, data.Ip, ci, data.Region);

                // Success result
                var result = ActionResult.Success;

                // Update refresh token and format result
                var refreshToken = await FormatLoginResultAsync(result, token, data.Device);

                // Return
                return (result, refreshToken);
            }
            else
            {
                return (ApplicationErrors.NoPasswordMatch.AsResult(), null);
            }
        }

        /// <summary>
        /// Refresh token (Related with Login, make sure the logic is consistent)
        /// 刷新令牌 (和登录相关，确保逻辑一致)
        /// </summary>
        /// <param name="token">Refresh token</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        public async ValueTask<(IActionResult, string?)> RefreshTokenAsync(string token, RefreshTokenDto model)
        {
            try
            {
                // Validate the token first
                // Expired then password should be valid
                var (claims, expired, _, _) = _authService.ValidateToken(token);
                if (claims == null)
                {
                    return (ApplicationErrors.NoValidData.AsResult("Claims"), null);
                }

                var refreshToken = RefreshToken.Create(claims);
                if (refreshToken == null || (expired && string.IsNullOrEmpty(model.Pwd)))
                {
                    return (ApplicationErrors.TokenExpired.AsResult(), null);
                }

                // Token IP should be the same
                if (!refreshToken.ClientIp.Equals(model.Ip))
                {
                    return (ApplicationErrors.IPAddressChanged.AsResult(), null);
                }

                // View the user's refresh token for matching
                var userId = refreshToken.Id;
                var tokenResult = await Repo.GetDeviceTokenAsync(userId, model.Device);
                if (tokenResult == null)
                {
                    return (ApplicationErrors.NoDeviceMatch.AsResult(), null);
                }

                var (deviceToken, user) = tokenResult.Value;
                if (deviceToken == null)
                {
                    return (ApplicationErrors.NoDeviceMatch.AsResult(), null);
                }

                // Has password or not
                if (!string.IsNullOrEmpty(model.Pwd))
                {
                    // Hashed password
                    var hashedPassword = await HashPasswordAsync(user.Id, model.Pwd);

                    // Password match
                    if (!user.Password.Equals(hashedPassword))
                    {
                        await Repo.AddLoginFailureAsync(user.Id, user.Failure);

                        // Add audit
                        await Repo.AddAuditAsync(AuditKind.TokenLogin, user.Id, "Token Login", new { model.Device, Success = false }, model.Ip, AuditFlag.Warning);

                        return (ApplicationErrors.NoPasswordMatch.AsResult(), null);
                    }
                }

                // Token match
                var hashedToken = await App.HashPasswordAsync(token);
                if (hashedToken == null || hashedToken != deviceToken.Token)
                {
                    return (ApplicationErrors.TokenExpired.AsResult("NoMatch"), null);
                }

                // User check
                if (!ServiceUtils.CheckUser(user, out var checkResult))
                {
                    return (checkResult, null);
                }

                // Current culture
                var ci = CultureInfo.CurrentCulture;

                // Service user
                var serviceUser = new ServiceUser(user.Role, userId, model.Ip, ci, refreshToken.Region, refreshToken.Organization, null, refreshToken.DeviceId);

                // Add audit
                await Repo.AddAuditAsync(AuditKind.TokenLogin, userId, "Token Login", new { model.Device, Success = true }, model.Ip, AuditFlag.Normal);

                // Success result
                var result = ActionResult.Success;

                // Update refresh token and format result
                var newToken = await FormatLoginResultAsync(result, serviceUser, model.Device);

                // Return
                return (result, newToken);
            }
            catch (Exception ex)
            {
                // Return action result
                return (LogException(ex), null);
            }
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
