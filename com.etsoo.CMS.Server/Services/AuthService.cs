using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Server;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using Dapper;
using System.Globalization;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Authorization service
    /// 授权服务
    /// </summary>
    public class AuthService : CommonService, IAuthService
    {
        readonly IPAddress ip;
        readonly CoreFramework.Authentication.IAuthService _authService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        public AuthService(IMyApp app, IMyUserAccessor userAccessor, ILogger<AuthService> logger)
            : base(app, userAccessor.User, "auth", logger)
        {
            if (app.AuthService == null) throw new NullReferenceException(nameof(app.AuthService));
            _authService = app.AuthService;
            ip = userAccessor.Ip;
        }

        /// <summary>
        /// Add login failure
        /// 增加登录失败
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="currentFailure">Current failure</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task AddLoginFailureAsync(string id, int? currentFailure, CancellationToken cancellationToken = default)
        {
            var failure = currentFailure.GetValueOrDefault() + 1;
            DateTime? frozenTime = null;
            if (failure >= 6)
            {
                frozenTime = DateTime.UtcNow.AddMinutes(15 * (failure / 6));
            }

            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));
            parameters.Add(nameof(failure), failure);

            if (frozenTime.HasValue)
            {
                parameters.Add(nameof(frozenTime), frozenTime.Value.ToString("u"));
            }
            else
            {
                parameters.Add(nameof(frozenTime), null);
            }

            var command = CreateCommand($"UPDATE users SET failure = @{nameof(failure)}, frozenTime = @{nameof(frozenTime)} WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);
            await QueryAsAsync<DbUser>(command);
        }

        // Hash password
        private async Task<string> HashPasswordAsync(string id, string password)
        {
            return await App.HashPasswordAsync(id + password);
        }

        private async Task<string> FormatLoginResultAsync(IActionResult result, IServiceUser user, string device, CancellationToken cancellationToken = default)
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
            await UpdateTokenAsync(user.Id, device, hashedToken, cancellationToken);

            // Return
            return refreshToken;
        }

        /// <summary>
        /// Get device token
        /// 获取设备令牌数据
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="device">Device</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Device token and user</returns>
        private async Task<(DbDevice Device, DbUser User)?> GetDeviceTokenAsync(string id, string device, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));
            parameters.Add(nameof(device), device.ToDbString(true, 128));

            var command = CreateCommand($"SELECT d.token, d.creation, u.id, u.password, u.role, u.status, u.frozenTime FROM devices AS d INNER JOIN users AS u ON d.user = u.id WHERE d.user = @{nameof(id)} AND d.device = @{nameof(device)}", parameters, cancellationToken: cancellationToken);
            return (await App.DB.WithConnection((connection) =>
            {
                return connection.QueryAsync<DbDevice, DbUser, (DbDevice, DbUser)>(command, (d, u) => (d, u), "id");
            }, cancellationToken)).FirstOrDefault();
        }

        /// <summary>
        /// Get user data
        /// 获取用户数据
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User data and is setup needed</returns>
        private async Task<(DbUser?, bool)> GetUserAsync(string id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add("id", id.ToDbString(true, 128));

            try
            {
                var command = CreateCommand($"SELECT id, password, role, status, failure, frozenTime FROM users WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);
                return (await QueryAsAsync<DbUser>(command), false);
            }
            catch
            {
                // Setup would be needed when failed with no table 'users'
                var setup = false;
                if (id.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    // https://www.sqlite.org/schematab.html
                    var command = CreateCommand("SELECT COUNT(*) FROM sqlite_schema WHERE type = 'table' AND name = 'users'", cancellationToken: cancellationToken);
                    setup = (await ExecuteScalarAsync<int>(command)) == 0;
                }
                return (null, setup);
            }
        }

        /// <summary>
        /// User login
        /// 用户登录
        /// </summary>
        /// <param name="data">Login data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(IActionResult Result, string? RefreshToken)> LoginAsync(LoginDto data, CancellationToken cancellationToken = default)
        {
            // Hashed password
            var hashedPassword = await HashPasswordAsync(data.Id, data.Pwd);

            // Get user data
            var (user, setup) = await GetUserAsync(data.Id, cancellationToken);
            if (user == null)
            {
                if (setup)
                {
                    try
                    {
                        // Setup
                        await SetupAsync(data.Id, hashedPassword, cancellationToken);

                        // Read the user again
                        (user, setup) = await GetUserAsync(data.Id, cancellationToken);
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

                await AddLoginFailureAsync(user.Id, user.Failure, cancellationToken);
            }
            else
            {
                success = true;
            }

            // Add audit
            var auditTitle = success ? Resources.LoginSuccess : Resources.LoginFailed;
            await AddAuditAsync(AuditKind.Login, user.Id, auditTitle, new Dictionary<string, object> { ["Device"] = data.Device, ["Success"] = success }, null, data.Ip, success ? AuditFlag.Normal : AuditFlag.Warning, cancellationToken: cancellationToken);

            if (success)
            {
                // Current culture
                var ci = CultureInfo.CurrentCulture;

                // Create token user from result data
                var token = new ServiceUser(user.Role, user.Id, data.Ip, ci, data.Region);

                // Success result
                var result = ActionResult.Success;

                // Update refresh token and format result
                var refreshToken = await FormatLoginResultAsync(result, token, data.Device, cancellationToken);

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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(IActionResult, string?)> RefreshTokenAsync(string token, RefreshTokenDto model, CancellationToken cancellationToken = default)
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
                var tokenResult = await GetDeviceTokenAsync(userId, model.Device, cancellationToken);
                if (tokenResult == null)
                {
                    return (ApplicationErrors.NoDeviceMatch.AsResult(), null);
                }

                var (deviceToken, user) = tokenResult.Value;
                if (deviceToken == null)
                {
                    return (ApplicationErrors.NoDeviceMatch.AsResult(), null);
                }

                // Audit title
                var auditTitle = Resources.TokenLogin;

                // Has password or not
                if (!string.IsNullOrEmpty(model.Pwd))
                {
                    // Hashed password
                    var hashedPassword = await HashPasswordAsync(user.Id, model.Pwd);

                    // Password match
                    if (!user.Password.Equals(hashedPassword))
                    {
                        await AddLoginFailureAsync(user.Id, user.Failure, cancellationToken);

                        // Add audit
                        await AddAuditAsync(AuditKind.TokenLogin, user.Id, auditTitle, new Dictionary<string, object> { ["Device"] = model.Device, ["Success"] = false }, null, model.Ip, AuditFlag.Warning, cancellationToken: cancellationToken);

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
                await AddAuditAsync(AuditKind.TokenLogin, userId, auditTitle, new Dictionary<string, object> { ["Device"] = model.Device, ["Success"] = true }, null, model.Ip, AuditFlag.Normal, cancellationToken: cancellationToken);

                // Success result
                var result = ActionResult.Success;

                // Update refresh token and format result
                var newToken = await FormatLoginResultAsync(result, serviceUser, model.Device, cancellationToken);

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
        /// Setup system
        /// 初始化系统
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="password">Password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task SetupAsync(string id, string password, CancellationToken cancellationToken = default)
        {
            // Create schema
            // https://www.sqlite.org/lang_createtable.html
            var command = CreateCommand(@$"
                /*
                    users table
                */
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    password TEXT NOT NULL,
                    role INTEGER NOT NULL,
                    status INTEGER NOT NULL,
                    failure INTEGER,
                    frozenTime TEXT,
                    creation TEXT NOT NULL,
                    refreshTime TEXT
                ) WITHOUT ROWID;

                CREATE INDEX IF NOT EXISTS index_users_refreshTime ON users (refreshTime);

                INSERT INTO users (id, password, role, status, creation) VALUES (@{nameof(id)}, @{nameof(password)}, 8192, 0, DATETIME('now', 'utc'));

                /*
                    device token table
                */
                CREATE TABLE IF NOT EXISTS devices (
                    user TEXT NOT NULL,
                    device TEXT NOT NULL,
                    token TEXT NOT NULL,
                    creation TEXT NOT NULL,
                    PRIMARY KEY (user, device)
                ) WITHOUT ROWID;

                CREATE INDEX IF NOT EXISTS index_devices_user ON devices (user);

                /*
                    audits table
                */
                CREATE TABLE IF NOT EXISTS audits (
                    kind INTEGER NOT NULL,
                    title TEXT NOT NULL,
                    content TEXT,
                    creation TEXT NOT NULL,
                    author TEXT NOT NULL,
                    target TEXT NOT NULL,
                    ip TEXT NOT NULL,
                    flag INTEGER DEFAULT 0,

                    FOREIGN KEY (author) REFERENCES users (id)
                );

                CREATE INDEX IF NOT EXISTS index_audits_author ON audits (author);
                CREATE INDEX IF NOT EXISTS index_audits_kind_target ON audits (kind, target);

                INSERT INTO audits (kind, title, creation, author, target, ip) VALUES (0, 'Initialize the website', DATETIME('now', 'utc'), @{nameof(id)}, @{nameof(id)}, @{nameof(ip)});

                /*
                    website table
                */
                CREATE TABLE IF NOT EXISTS website (
                    domain TEXT NOT NULL,
                    title TEXT NOT NULL,
                    keywords TEXT,
                    description TEXT,
                    version TEXT,
                    jsonData TEXT
                );

                /*
                    resources table
                */
                CREATE TABLE IF NOT EXISTS resources (
                    id TEXT PRIMARY KEY,
                    value TEXT NOT NULL
                ) WITHOUT ROWID;

                /*
                    services table
                */
                CREATE TABLE IF NOT EXISTS services (
                    id TEXT PRIMARY KEY,
                    app TEXT NOT NULL,
                    secret TEXT,
                    status INTEGER NOT NULL,
                    refreshTime TEXT NOT NULL,
                    jsonData TEXT
                ) WITHOUT ROWID;

                /*
                    tabs table
                */
                CREATE TABLE IF NOT EXISTS tabs (
                    id INTEGER PRIMARY KEY,
                    parent INTEGER,
                    name TEXT NOT NULL,
                    url TEXT NOT NULL,
                    refreshTime TEXT NOT NULL,
                    status INTEGER NOT NULL,
                    layout INTEGER NOT NULL,
                    orderIndex INTEGER NOT NULL,
                    articles INTEGER NOT NULL,
                    template TEXT,
                    logo TEXT,
                    description TEXT,
                    jsonData TEXT,
                    icon TEXT,

                    FOREIGN KEY (parent) REFERENCES tabs (id)
                );

                CREATE INDEX IF NOT EXISTS index_tabs_parent ON tabs (parent, orderIndex);
                CREATE INDEX IF NOT EXISTS index_tabs_url ON tabs (url);

                /*
                    articles table
                */
                CREATE TABLE IF NOT EXISTS articles (
                    id INTEGER PRIMARY KEY,
                    title TEXT NOT NULL,
                    subtitle TEXT,
                    keywords TEXT,
                    description TEXT,
                    url TEXT NOT NULL,
                    content TEXT NOT NULL,
                    logo TEXT,
                    tab1 INTEGER NOT NULL,
                    tab2 INTEGER,
                    tab3 INTEGER,
                    weight INTEGER NOT NULL,
                    year INTERGER NOT NULL,
                    creation TEXT NOT NULL,
                    release TEXT NOT NULL,
                    refreshTime TEXT NOT NULL,
                    author TEXT NOT NULL,
                    status INTEGER NOT NULL,
                    orderIndex INTEGER NOT NULL,
                    slideshow TEXT,
                    jsonData TEXT,

                    FOREIGN KEY (author) REFERENCES users (id)
                );

                CREATE INDEX IF NOT EXISTS index_articles_primary ON articles (tab1, tab2, tab3, orderIndex, release, weight, status);
                CREATE INDEX IF NOT EXISTS index_articles_author ON articles (author);
                CREATE INDEX IF NOT EXISTS index_articles_url ON articles (url, year);

                /*
                    files table
                */
                CREATE TABLE IF NOT EXISTS files (
                    id TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    path TEXT NOT NULL,
                    size INTEGER NOT NULL,
                    contentType TEXT NOT NULL,
                    shared INTEGER DEFAULT 0,
                    author TEXT NOT NULL,
                    creation TEXT NOT NULL
                ) WITHOUT ROWID;

                CREATE INDEX IF NOT EXISTS index_files_name ON files (name);
                CREATE INDEX IF NOT EXISTS index_files_author ON files (author);
                CREATE INDEX IF NOT EXISTS index_files_creation ON files (creation);
            ", new DbParameters(new { id, password, ip = ip.ToString() }), cancellationToken: cancellationToken);

            await ExecuteAsync(command);
        }

        /// <summary>
        /// Update device refresh token
        /// 更新设备更新令牌
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="device">Device</param>
        /// <param name="token">Refresh token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task UpdateTokenAsync(string id, string device, string token, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));
            parameters.Add(nameof(device), device.ToDbString(true, 128));
            parameters.Add(nameof(token), token.ToDbString(true, 256));

            var now = DateTime.UtcNow.ToString("u");
            parameters.Add(nameof(now), now);

            var command = CreateCommand(@$"INSERT INTO devices (user, device, token, creation) VALUES (@{nameof(id)}, @{nameof(device)}, @{nameof(token)}, @{nameof(now)})
                ON CONFLICT DO UPDATE SET token = @{nameof(token)}, creation = @{nameof(now)};
                
                UPDATE users SET failure = 0, refreshTime = @{nameof(now)} WHERE id = @{nameof(id)}
            ", parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);
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
