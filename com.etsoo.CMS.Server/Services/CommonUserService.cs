using com.etsoo.CMS.Application;
using com.etsoo.CMS.Server.Defs;
using com.etsoo.CMS.Services;
using com.etsoo.CoreFramework.User;

namespace com.etsoo.CMS.Server.Services
{
    public abstract class CommonUserService : CommonService, ICommonUserService
    {
        /// <summary>
        /// Current user
        /// 当前用户
        /// </summary>
        protected override ServiceUser User { get; }

        protected CommonUserService(IMyApp app, ServiceUser user, string flag, ILogger logger)
            : base(app, user, flag, logger)
        {
            User = user;
        }
    }
}
