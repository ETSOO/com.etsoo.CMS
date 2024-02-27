using com.etsoo.CoreFramework.Services;
using com.etsoo.Utils.Actions;
using System.Net;
using System.Text.Json.Serialization.Metadata;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Common service interface
    /// 通用服务接口
    /// </summary>
    public interface ICommonService : IServiceBase
    {
        Task AddAuditAsync<T>(AuditKind kind, string target, string title, T? content, JsonTypeInfo<T>? typeInfo, IPAddress ip, AuditFlag flag = AuditFlag.Normal, string? id = null, CancellationToken cancellationToken = default);
        Task AddAuditAsync<T>(AuditKind kind, string target, string title, IPAddress ip, IActionResult result, T? content = default, JsonTypeInfo<T>? typeInfo = null, CancellationToken cancellationToken = default);
    }
}