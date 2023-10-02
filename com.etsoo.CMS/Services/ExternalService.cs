using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Service;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// External service
    /// 外部服务
    /// </summary>
    public class ExternalService : CommonService<ServiceRepo>, IExternalService
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="logger">Logger</param>
        public ExternalService(IMyApp app, ILogger<ExternalService> logger)
            : base(app, new ServiceRepo(app), logger)
        {
        }

        /// <summary>
        /// Get article
        /// 获取文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response"></param>
        /// <returns>Result</returns>
        public async Task GetArticleAsync(GetArticleRQ rq, HttpResponse response)
        {
            await Repo.GetArticleAsync(rq, response);
        }

        /// <summary>
        /// Get articles
        /// 获取文章列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response"></param>
        /// <returns>Result</returns>
        public async Task GetArticlesAsync(GetArticlesRQ rq, HttpResponse response)
        {
            await Repo.GetArticlesAsync(rq, response);
        }

        /// <summary>
        /// Get slideshow articles
        /// 获取幻灯片文章
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Result</returns>
        public async Task GetSlideshowsAsync(HttpResponse response)
        {
            await Repo.GetSlideshowsAsync(response);
        }

        /// <summary>
        /// Get website data
        /// 获取网站数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task GetSiteDataAsync(HttpResponse response)
        {
            await Repo.GetSiteDataAsync(response);
        }
    }
}
