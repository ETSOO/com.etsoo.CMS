using com.etsoo.CMS.Application;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.ServiceApp.Services;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    /// <typeparam name="R">Generic repository type</typeparam>
    public abstract class CommonService<R> : SqliteService<IMyApp, R>
        where R : IRepoBase
    {
        protected CommonService(IMyApp app, R repo, ILogger logger) : base(app, repo, logger)
        {
        }
    }
}
