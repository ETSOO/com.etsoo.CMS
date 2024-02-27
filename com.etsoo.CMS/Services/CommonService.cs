using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CoreFramework.Services;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.Application;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using Microsoft.Data.Sqlite;
using System.Net;
using System.Text.Json.Serialization.Metadata;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    public abstract class CommonService : ServiceBase<MyAppConfiguration, SqliteConnection, IMyApp, ServiceUser>, ICommonService
    {
        /// <summary>
        /// Current user parameter name
        /// 当前用户参数名称
        /// </summary>
        protected const string SysUserField = $"@{Constants.CurrentUserField}";

        protected CommonService(IMyApp app, ServiceUser? user, string flag, ILogger logger)
            : base(app, user, flag, logger)
        {
        }

        /// <summary>
        /// Add audit
        /// 添加审计
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="target">Target</param>
        /// <param name="title">Title</param>
        /// <param name="content">Content</param>
        /// <param name="typeInfo">Content Json type info</param>
        /// <param name="ip">IP</param>
        /// <param name="flag">Flag</param>
        /// <param name="id">Author</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AddAuditAsync<T>(AuditKind kind, string target, string title, T? content, JsonTypeInfo<T>? typeInfo, IPAddress ip, AuditFlag flag = AuditFlag.Normal, string? id = null, CancellationToken cancellationToken = default)
        {
            id ??= User?.Id ?? "admin";

            string? json;
            if (content == null) json = null;
            else if (content is string jsonContent) json = jsonContent;
            else if (typeInfo != null) json = await SharedUtils.JsonSerializeAsync(content, typeInfo);
            else json = await SharedUtils.JsonSerializeAsync(content, App.DefaultJsonSerializerOptions);

            var parameters = new DbParameters();
            parameters.Add(nameof(kind), (byte)kind);
            parameters.Add(nameof(id), id);
            parameters.Add(nameof(title), title.ToDbString(false, 258));
            parameters.Add(nameof(content), json);
            parameters.Add(nameof(ip), ip.ToString());
            parameters.Add(nameof(flag), (byte)flag);
            parameters.Add(nameof(target), target.ToDbString(true, 128));

            var now = DateTime.UtcNow.ToString("u").ToDbString(true, 128);
            parameters.Add(nameof(now), now);

            // Keep the traditional way
            var command = CreateCommand($"INSERT INTO audits (kind, title, content, creation, author, target, ip, flag) VALUES (@{nameof(kind)}, @{nameof(title)}, @{nameof(content)}, @{nameof(now)}, @{nameof(id)}, @{nameof(target)}, @{nameof(ip)}, @{nameof(flag)})", parameters, cancellationToken: cancellationToken);
            await ExecuteAsync(command);
        }

        /// <summary>
        /// Add audit
        /// 添加审计
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="target">Target</param>
        /// <param name="title">Title</param>
        /// <param name="ip">IP</param>
        /// <param name="result">Action result</param>
        /// <param name="content">Content</param>
        /// <param name="typeInfo">Content Json type info</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task AddAuditAsync<T>(AuditKind kind, string target, string title, IPAddress ip, IActionResult result, T? content = default, JsonTypeInfo<T>? typeInfo = null, CancellationToken cancellationToken = default)
        {
            if (result.Ok)
            {
                await AddAuditAsync(kind, target, title, content, typeInfo, ip, AuditFlag.Normal, cancellationToken: cancellationToken);
            }
            else
            {
                await AddAuditAsync(kind, target, $"{title} - {result.Title}", content, typeInfo, ip, AuditFlag.Warning, cancellationToken: cancellationToken);
            }
        }
    }
}