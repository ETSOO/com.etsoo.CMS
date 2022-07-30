using com.etsoo.ServiceApp.Application;
using com.etsoo.ServiceApp.Repo;
using com.etsoo.ServiceApp.Services;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    /// <typeparam name="R">Generic repository</typeparam>
    public abstract class CommonService<R> : SqliteService<R> where R : SqliteRepo
    {
        /// <summary>
        /// Audit kind
        /// 审计类型
        /// </summary>
        protected enum AuditKind
        {
            Init,
            Login
        }

        protected CommonService(ISqliteApp app, R repo, ILogger logger) : base(app, repo, logger)
        {
        }

        protected async Task AddAudit(string username, IPAddress ip)
        {

        }
    }
}
