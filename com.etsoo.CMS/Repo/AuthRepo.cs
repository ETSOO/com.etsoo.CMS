using com.etsoo.CMS.Models;
using com.etsoo.Database;
using com.etsoo.ServiceApp.Application;
using Dapper;
using System.Net;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Authorization repository
    /// 授权仓库
    /// </summary>
    public class AuthRepo : CommonRepo
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        public AuthRepo(ISqliteApp app)
            : base(app, "auth")
        {
        }

        /// <summary>
        /// Add login failure
        /// 增加登录失败
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="currentFailure">Current failure</param>
        /// <returns>Task</returns>
        public async Task AddLoginFailureAsync(string id, int? currentFailure)
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

            var command = CreateCommand($"UPDATE users SET failure = @{nameof(failure)}, frozenTime = @{nameof(frozenTime)} WHERE id = @{nameof(id)}", parameters);
            await QueryAsAsync<DbUser>(command);
        }

        /// <summary>
        /// Get device token
        /// 获取设备令牌数据
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="device">Device</param>
        /// <returns>Device token and user</returns>
        public async Task<(DbDevice Device, DbUser User)?> GetDeviceTokenAsync(string id, string device)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 128));
            parameters.Add(nameof(device), device.ToDbString(true, 128));

            var command = CreateCommand($"SELECT d.token, d.creation, u.id, u.password, u.role, u.status, u.frozenTime FROM devices AS d INNER JOIN users AS u ON d.user = u.id WHERE d.user = @{nameof(id)} AND d.device = @{nameof(device)}", parameters);
            return (await App.DB.WithConnection((connection) =>
            {
                return connection.QueryAsync<DbDevice, DbUser, (DbDevice, DbUser)>(command, (d, u) => (d, u), "id");
            })).FirstOrDefault();
        }

        /// <summary>
        /// Get user data
        /// 获取用户数据
        /// </summary>
        /// <param name="id">Username</param>
        /// <returns>User data and is setup needed</returns>
        public async Task<(DbUser?, bool)> GetUserAsync(string id)
        {
            var parameters = new DbParameters();
            parameters.Add("id", id.ToDbString(true, 128));

            try
            {
                var command = CreateCommand($"SELECT id, password, role, status, failure, frozenTime FROM users WHERE id = @{nameof(id)}", parameters);
                return (await QueryAsAsync<DbUser>(command), false);
            }
            catch
            {
                // Setup would be needed when failed with no table 'users'
                var setup = false;
                if (id.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    // https://www.sqlite.org/schematab.html
                    var command = CreateCommand("SELECT COUNT(*) FROM sqlite_schema WHERE type = 'table' AND name = 'users'");
                    setup = (await ExecuteScalarAsync<int>(command)) == 0;
                }
                return (null, setup);
            }
        }

        /// <summary>
        /// Setup system
        /// 初始化系统
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="password">Password</param>
        /// <param name="ip">Ip</param>
        /// <returns>Task</returns>
        public async Task SetupAsync(string id, string password, IPAddress ip)
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

                INSERT INTO users (id, password, role, status, creation) VALUES (@{nameof(id)}, @{nameof(password)}, 8192, 0, DATETIME('now'));

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

                INSERT INTO audits (kind, title, creation, author, target, ip) VALUES (0, 'Initialize the website', DATETIME('now'), @{nameof(id)}, @{nameof(id)}, @{nameof(ip)});

                /*
                    website table
                */
                CREATE TABLE IF NOT EXISTS website (
                    domain TEXT NOT NULL,
                    title TEXT NOT NULL,
                    keywords TEXT,
                    description TEXT,
                    version TEXT
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
                    refreshTime TEXT NOT NULL
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

                    FOREIGN KEY (author) REFERENCES users (id)
                );

                CREATE INDEX IF NOT EXISTS index_articles_primary ON articles (tab1, tab2, tab3, orderIndex, release, weight, status);
                CREATE INDEX IF NOT EXISTS index_articles_author ON articles (author);
                CREATE INDEX IF NOT EXISTS index_articles_url ON articles (url, year);
            ", new DbParameters(new { id, password, ip = ip.ToString() }));

            await ExecuteAsync(command);
        }

        /// <summary>
        /// Update device refresh token
        /// 更新设备更新令牌
        /// </summary>
        /// <param name="id">Username</param>
        /// <param name="device">Device</param>
        /// <param name="token">Refresh token</param>
        /// <returns>Task</returns>
        public async Task UpdateTokenAsync(string id, string device, string token)
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
            ", parameters);
            await ExecuteAsync(command);
        }
    }
}
