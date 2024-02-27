using com.etsoo.CMS.RQ.Service;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// External service interface
    /// 外部服务接口
    /// </summary>
    public interface IExternalService : ICommonService
    {
        Task GetArticleAsync(GetArticleRQ rq, HttpResponse response, CancellationToken cancellationToken = default);

        Task GetArticlesAsync(GetArticlesRQ rq, HttpResponse response, CancellationToken cancellationToken = default);

        Task GetSlideshowsAsync(HttpResponse response, CancellationToken cancellationToken = default);

        Task GetSiteDataAsync(HttpResponse response, CancellationToken cancellationToken = default);
    }
}
