using com.etsoo.CMS.RQ.Service;
using com.etsoo.CoreFramework.Services;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// External service interface
    /// 外部服务接口
    /// </summary>
    public interface IExternalService : IServiceBase
    {
        /// <summary>
        /// Get article
        /// 获取文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response"></param>
        /// <returns>Result</returns>
        Task GetArticleAsync(GetArticleRQ rq, HttpResponse response);

        /// <summary>
        /// Get slideshow articles
        /// 获取幻灯片文章
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Result</returns>
        Task GetSlideshowsAsync(HttpResponse response);

        /// <summary>
        /// Get website data
        /// 获取网站数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task GetSiteDataAsync(HttpResponse response);
    }
}
