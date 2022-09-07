using com.etsoo.CMS.Defs;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.Application;
using com.etsoo.ServiceApp.Repo;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using System.Net;

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
        /// <param name="target">Target</param>
        /// <param name="title">Title</param>
        /// <param name="content">Content, obj or Json string</param>
        /// <param name="ip">IP</param>
        /// <param name="flag">Flag</param>
        /// <param name="id">Author</param>
        /// <returns>Task</returns>
        public async Task AddAuditAsync(AuditKind kind, string target, string title, object? content, IPAddress ip, AuditFlag flag = AuditFlag.Normal, string? id = null)
        {
            id ??= User?.Id ?? "admin";

            string? json;
            if (content == null) json = null;
            else if (content is string jsonContent) json = jsonContent;
            else json = await SharedUtils.JsonSerializeAsync(content, App.DefaultJsonSerializerOptions);

            var parameters = new DbParameters();
            parameters.Add(nameof(kind), (byte)kind);
            parameters.Add(nameof(id), id);
            parameters.Add(nameof(title), title.ToDbString(false, 258));
            parameters.Add(nameof(content), json);
            parameters.Add(nameof(ip), ip.ToString());
            parameters.Add(nameof(flag), (byte)flag);
            parameters.Add(nameof(target), target.ToDbString(true, 128));

            var now = DateTime.UtcNow.ToString("s").ToDbString(true, 128);
            parameters.Add(nameof(now), now);

            var command = CreateCommand($"INSERT INTO audits (kind, title, content, creation, author, target, ip, flag) VALUES (@{nameof(kind)}, @{nameof(title)}, @{nameof(content)}, @{nameof(now)}, @{nameof(id)}, @{nameof(target)}, @{nameof(ip)}, @{nameof(flag)})",
                parameters);
            await ExecuteAsync(command);
        }

        /// <summary>
        /// Add audit
        /// 添加审计
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="target">Target</param>
        /// <param name="title">Title</param>
        /// <param name="result">Action result</param>
        /// <param name="content">Content</param>
        /// <returns>Task</returns>
        public async Task AddAuditAsync(AuditKind kind, string target, string title, IPAddress ip, IActionResult result, object? content = null)
        {
            if (result.Ok)
            {
                await AddAuditAsync(kind, target, title, content, ip, AuditFlag.Normal);
            }
            else
            {
                await AddAuditAsync(kind, target, $"{title} - {result.Title}", content, ip, AuditFlag.Warning);
            }
        }
    }
}
