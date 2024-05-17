using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CMS.Server.Defs;
using com.etsoo.CMS.Server.RQ.Article;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Website article service interface
    /// 网站文章业务逻辑服务接口
    /// </summary>
    public interface IArticleService : ICommonUserService
    {
        Task<IActionResult> CreateAsync(ArticleCreateRQ rq, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<IActionResult> DeletePhotoAsync(ArticleDeletePhotoRQ rq, CancellationToken cancellationToken = default);

        Task<DbArticleQuery[]> QueryAsync(ArticleQueryRQ rq, CancellationToken cancellationToken = default);

        Task HistoryAsync(ArticleHistoryQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default);

        Task<IActionResult> SortPhotosAsync(ArticleSortPhotosRQ rq, CancellationToken cancellationToken = default);

        Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdateAsync(ArticleUpdateRQ rq, CancellationToken cancellationToken = default);

        ValueTask<string?> UploadLogoAsync(int id, Stream logoStream, string contentType, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdatePhotoAsync(ArticleUpdatePhotoRQ rq, CancellationToken cancellationToken = default);

        Task<IActionResult> UploadPhotosAsync(int id, IEnumerable<IFormFile> files, CancellationToken cancellationToken = default);

        Task UpdateReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default);

        Task ViewGalleryAsync(int id, HttpResponse response, CancellationToken cancellationToken = default);

        Task<string?> ViewGalleryAsync(int id, CancellationToken cancellationToken = default);

        Task ViewReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default);
    }
}
