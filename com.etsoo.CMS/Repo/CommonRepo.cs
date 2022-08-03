using com.etsoo.CMS.Defs;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.Application;
using com.etsoo.ServiceApp.Repo;
using Dapper;
using System.Net;
using System.Text.Json;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Common repository
    /// 通用数据仓库
    /// </summary>
    public abstract class CommonRepo : SqliteRepo
    {
        /// <summary>
        /// Current user parameter name
        /// 当前用户参数名称
        /// </summary>
        protected const string SysUserField = $"@{Constants.CurrentUserField}";

        protected CommonRepo(ISqliteApp app, string flag, IServiceUser? user = null) : base(app, flag, user)
        {
        }

        /// <summary>
        /// Add audit
        /// 添加审计
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="username">Username</param>
        /// <param name="title">Title</param>
        /// <param name="content">Content</param>
        /// <param name="ip">IP</param>
        /// <param name="flag">Flag</param>
        /// <returns>Task</returns>
        public async Task AddAuditAsync(AuditKind kind, string username, string title, object? content, IPAddress ip, AuditFlag flag = AuditFlag.Normal)
        {
            var json = content == null ? null : JsonSerializer.Serialize(content, App.DefaultJsonSerializerOptions);

            var command = CreateCommand("INSERT INTO audits (kind, title, content, creation, author, ip, flag) VALUES (@kind, @title, @content, @now, @id, @ip, @flag)",
                new DynamicParameters(new
                {
                    kind = (byte)kind,
                    id = username,
                    title,
                    content = json,
                    ip = ip.ToString(),
                    flag = (byte)flag,
                    now = DateTime.UtcNow.ToString("s")
                }));
            await ExecuteAsync(command);
        }
    }
}
