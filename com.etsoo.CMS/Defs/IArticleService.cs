using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Website article service interface
    /// 网站文章业务逻辑服务接口
    /// </summary>
    public interface IArticleService : IServiceBase
    {
        /// <summary>
        /// Create article
        /// 创建文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> CreateAsync(ArticleCreateRQ rq, IPAddress ip);

        /// <summary>
        /// Delete photo
        /// 删除照片
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        Task<IActionResult> DeletePhotoAsync(ArticleDeletePhotoRQ rq, IPAddress ip);

        /// <summary>
        /// Query article
        /// 查询文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        Task<DbArticleQuery[]> QueryAsync(ArticleQueryRQ rq);

        /// <summary>
        /// Query history
        /// 查询操作历史
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task QueryHistoryAsync(int id, HttpResponse response);

        /// <summary>
        /// Sort gallery photos
        /// 图库照片排序
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        Task<IActionResult> SortPhotosAsync(ArticleSortPhotosRQ rq, IPAddress ip);

        /// <summary>
        /// Translate
        /// 翻译
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Result</returns>
        Task<string> TranslateAsync(string text);

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> UpdateAsync(ArticleUpdateRQ rq, IPAddress ip);

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="logoStream">Logo stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="ip">IP address</param>
        /// <returns>New URL</returns>
        ValueTask<string?> UploadLogoAsync(int id, Stream logoStream, string contentType, IPAddress ip);

        /// <summary>
        /// Update photo gallery item
        /// 更新图片库项目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> UpdatePhotoAsync(ArticleUpdatePhotoRQ rq, IPAddress ip);

        /// <summary>
        /// Async upload photo files
        /// 异步上传照片文件
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="files">Photo files</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        Task<IActionResult> UploadPhotosAsync(int id, IEnumerable<IFormFile> files, IPAddress ip);

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task UpdateReadAsync(int id, HttpResponse response);

        /// <summary>
        /// Read for gallery photos
        /// 阅读图库照片
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task ViewGalleryAsync(int id, HttpResponse response);

        /// <summary>
        /// Read for view
        /// 阅读浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task ViewReadAsync(int id, HttpResponse response);
    }
}
