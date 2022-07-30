using com.etsoo.CMS.Models;
using com.etsoo.ServiceApp.Application;
using com.etsoo.ServiceApp.Repo;
using Dapper;
using System.Net;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Authorization repository
    /// 授权仓库
    /// </summary>
    public class AuthRepo : SqliteRepo
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
        /// Get user data
        /// 获取用户数据
        /// </summary>
        /// <param name="id">Username</param>
        /// <returns>User data and is setup needed</returns>
        public async Task<(DbUser?, bool)> GetUserAsync(string id)
        {
            try
            {
                var command = CreateCommand("SELECT id, password, status, frozenTime FROM users WHERE id = @id", new DynamicParameters(new { id }));
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

        public async Task SetupAsync(string id, string password, IPAddress ip)
        {
            // Create schema
            // https://www.sqlite.org/lang_createtable.html
            var command = CreateCommand(@"
                /*
                    users table
                */
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    password TEXT NOT NULL,
                    status INTEGER NOT NULL,
                    frozenTime TEXT
                ) WITHOUT ROWID;

                INSERT INTO users (id, password, status) VALUES (@id, @password, 0);

                /*
                    audits table
                */
                CREATE TABLE IF NOT EXISTS audits (
                    kind TEXT NOT NULL,
                    title TEXT NOT NULL,
                    content TEXT,
                    creation TEXT NOT NULL,
                    author TEXT,
                    ip TEXT,

                    FOREIGN KEY (author) REFERENCES users (id)
                );

                INSERT INTO users (kind, title, creation, author, ip) VALUES ('Init', 'Initialize the website', @now, @id, @ip);

                /*
                    website table
                */
                CREATE TABLE IF NOT EXISTS website (
                    title TEXT NOT NULL,
                    keywords TEXT NOT NULL,
                    description TEXT NOT NULL
                );

                /*
                    services table
                */
                CREATE TABLE IF NOT EXISTS services (
                    id TEXT PRIMARY KEY,
                    app TEXT NOT NULL,
                    secret TEXT
                ) WITHOUT ROWID;

                /*
                    tabs table
                */
                CREATE TABLE IF NOT EXISTS tabs (
                    name TEXT NOT NULL,
                    url TEXT NOT NULL,
                    orderIndex INTEGER NOT NULL
                );

                /*
                    articles table
                */
                CREATE TABLE IF NOT EXISTS articles (
                    title TEXT NOT NULL,
                    url TEXT NOT NULL,
                    content TEXT NOT NULL,
                    creation TEXT NOT NULL,
                    author TEXT NOT NULL,
                    orderIndex INTEGER NOT NULL,

                    FOREIGN KEY (author) REFERENCES users (id)
                );
            ", new DynamicParameters(new { id, password, ip = ip.ToString(), now = DateTime.UtcNow.ToString("s") }));

            await ExecuteAsync(command);
        }
    }
}
