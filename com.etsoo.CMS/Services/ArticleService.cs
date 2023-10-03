using com.etsoo.ApiProxy.Defs;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.HtmlIO;
using com.etsoo.HTTP;
using com.etsoo.ImageUtils;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using SixLabors.ImageSharp;
using System.Collections.Concurrent;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website article service
    /// 网站文章业务逻辑服务
    /// </summary>
    public class ArticleService : CommonService<ArticleRepo>, IArticleService
    {
        readonly IBridgeProxy bridgeProxy;
        readonly IStorage storage;
        readonly IWebsiteService websiteService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="bridgeProxy">Bridge proxy</param>
        /// <param name="storage">Storage</param>
        public ArticleService(IMyApp app, IServiceUserAccessor userAccessor, ILogger<ArticleService> logger, IBridgeProxy bridgeProxy, IStorage storage, IWebsiteService websiteService)
            : base(app, new ArticleRepo(app, userAccessor.UserSafe), logger)
        {
            this.bridgeProxy = bridgeProxy;

            // Optional injection
            // IEnumerable<IStorage> storages
            // storage = storages.FirstOrDefault();
            this.storage = storage;

            this.websiteService = websiteService;
        }

        /// <summary>
        /// Create article
        /// 创建文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(ArticleCreateRQ rq, IPAddress ip)
        {
            if (!string.IsNullOrEmpty(rq.Keywords))
            {
                rq.Keywords = ServiceUtils.FormatKeywords(rq.Keywords);
            }

            rq.Content = await FormatContentAsync(rq.Content);

            var result = await Repo.CreateAsync(rq);
            var id = result.Data;
            await Repo.AddAuditAsync(AuditKind.CreateArticle, id.ToString(), $"Create article {id}", ip, result.Result, rq);

            await OnDemandRevalidateAsync(id);

            return result.MergeData("id");
        }

        /// <summary>
        /// Delete photo
        /// 删除照片
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> DeletePhotoAsync(ArticleDeletePhotoRQ rq, IPAddress ip)
        {
            var result = await Repo.DeletePhotoAsync(rq.Id, rq.Url);

            if (result.Ok)
            {
                await storage.DeleteUrlAsync(rq.Url);
                await OnDemandRevalidateAsync(rq.Id);
            }

            await Repo.AddAuditAsync(AuditKind.DeleteGalleryPhoto, rq.Id.ToString(), $"Delete article {rq.Id} photo", ip, result, rq);
            return result;
        }

        /// <summary>
        /// On-demand Revalidation
        /// 按需重新验证
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        protected async Task OnDemandRevalidateAsync(int id)
        {
            // Read article link data
            var link = await Repo.QueryLinkAsync(id);
            if (link == null) return;

            var url = link.GetUrl();
            if (string.IsNullOrEmpty(url)) return;

            // Website service
            await websiteService.OnDemandRevalidateAsync(url);
        }

        /// <summary>
        /// Query article
        /// 查询文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        public async Task<DbArticleQuery[]> QueryAsync(ArticleQueryRQ rq)
        {
            return await Repo.QueryAsync(rq);
        }

        /// <summary>
        /// Query history
        /// 查询操作历史
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryHistoryAsync(int id, HttpResponse response)
        {
            await Repo.QueryHistoryAsync(id, response);
        }

        /// <summary>
        /// Sort gallery photos
        /// 图库照片排序
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> SortPhotosAsync(ArticleSortPhotosRQ rq, IPAddress ip)
        {
            var items = await Repo.ViewGalleryItemsAsync(rq.Id);
            IActionResult result;
            if (items?.Any() is true)
            {
                // Map
                try
                {
                    var mapItems = rq.Ids.Select((id) => items.ElementAt(id));

                    foreach (var item in items)
                    {
                        if (!mapItems.Any(mapItem => mapItem.Url.Equals(item.Url)))
                        {
                            mapItems = mapItems.Append(item);
                        }
                    }

                    await Repo.SavePhotosAsync(rq.Id, mapItems);

                    await OnDemandRevalidateAsync(rq.Id);

                    result = ActionResult.Success;
                }
                catch (Exception ex)
                {
                    result = LogException(ex);
                }
            }
            else
            {
                result = ApplicationErrors.NoId.AsResult();
            }

            await Repo.AddAuditAsync(AuditKind.SortGalleryPhoto, rq.Id.ToString(), $"Sort article {rq.Id} photos", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Translate
        /// 翻译
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Result</returns>
        public async Task<string> TranslateAsync(string text)
        {
            return await bridgeProxy.TranslateTextAsync(new()
            {
                Text = text,
                TargetLanguageCode = "en",
                SourceLanguageCode = "zh"
            });
        }

        private async Task<string> FormatContentAsync(string content)
        {
            if (storage == null) return content;

            var path = $"/Resources/{DateTime.UtcNow:yyyyMM}/";
            return await HtmlIOUtils.FormatEditorContentAsync(storage, path, content, Logger, CancellationToken) ?? content;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(ArticleUpdateRQ rq, IPAddress ip)
        {
            if (!string.IsNullOrEmpty(rq.Url))
            {
                rq.Url = rq.Url.Trim('/');
            }

            if (!string.IsNullOrEmpty(rq.Content)
                && rq.ChangedFields?.Contains("content", StringComparer.OrdinalIgnoreCase) is true)
            {
                rq.Content = await FormatContentAsync(rq.Content);
            }

            var refreshTime = DateTime.UtcNow.ToString("u");
            var parameters = new Dictionary<string, object>
            {
                [nameof(refreshTime)] = refreshTime
            };

            var releaseStr = rq.Release?.ToUniversalTime().ToString("u");
            if (releaseStr != null)
            {
                parameters[nameof(rq.Release)] = releaseStr;
            }

            var (result, _) = await Repo.InlineUpdateAsync<int, UpdateModel<int>>(
                rq,
                new QuickUpdateConfigs(new[] { "title", "subtitle", "keywords", "description", "url", "content", "logo", "jsonData", "release", "tab1", "weight", "status", "slideshow" })
                {
                    TableName = "articles",
                    IdField = "id"
                },
                $"refreshTime = @{nameof(refreshTime)}", parameters
             );
            await Repo.AddAuditAsync(AuditKind.UpdateArticle, rq.Id.ToString(), $"Update article {rq.Id}", ip, result, rq);

            await OnDemandRevalidateAsync(rq.Id);

            return result;
        }

        /// <summary>
        /// Update photo gallery item
        /// 更新图片库项目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdatePhotoAsync(ArticleUpdatePhotoRQ rq, IPAddress ip)
        {
            var result = await Repo.UpdatePhotoAsync(rq);

            if (result.Ok && storage != null)
            {
                await OnDemandRevalidateAsync(rq.Id);
            }

            await Repo.AddAuditAsync(AuditKind.UpdateGalleryItem, rq.Id.ToString(), $"Update article {rq.Id} gallery photo item", ip, result, rq);
            return result;
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="logoStream">Logo stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="ip">IP address</param>
        /// <returns>New URL</returns>
        public async ValueTask<string?> UploadLogoAsync(int id, Stream logoStream, string contentType, IPAddress ip)
        {
            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            var logo = await Repo.ReadLogoAsync(id);
            if (logo == null)
            {
                return null;
            }

            // File path
            var path = $"/Resources/ArticleLogos/a{id.ToString().PadLeft(8, '0')}.{Path.GetRandomFileName()}{extension}";

            // Save the stream to file directly
            var saveResult = await storage.WriteAsync(path, logoStream, WriteCase.CreateNew);

            if (saveResult)
            {
                // New avatar URL
                var url = storage.GetUrl(path);

                // Repo update
                if (await Repo.UpdateLogoAsync(id, url) > 0)
                {
                    // Audit
                    await Repo.AddAuditAsync(AuditKind.UpdateArticleLogo, id.ToString(), $"Update article {id} logo", new { Logo = logo, NewLogo = url }, ip);

                    await OnDemandRevalidateAsync(id);

                    // Return
                    return url;
                }
            }

            Logger.LogError("Logo write path is {path}", path);

            await Repo.AddAuditAsync(AuditKind.UpdateArticleLogo, id.ToString(), $"Update article {id} logo", ip, new ActionResult(), new { Path = path });

            return null;
        }

        /// <summary>
        /// Async upload photo files
        /// 异步上传照片文件
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="files">Photo files</param>
        /// <param name="ip">IP address</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> UploadPhotosAsync(int id, IEnumerable<IFormFile> files, IPAddress ip)
        {
            var websiteRepo = new WebsiteRepo(App, Repo.User);
            var websiteData = await websiteRepo.ReadJsonDataAsync<JsonDataGalleryLogoSize>();
            var size = websiteData?.GalleryLogoSize;
            var width = size?.FirstOrDefault() ?? 800;
            var height = size?.Count() > 1 ? size.ElementAt(1) : 0;

            var exceptions = new ConcurrentQueue<Exception>();
            var photos = new ConcurrentQueue<GalleryPhotoExtendedDto>();

            // File path
            var path = $"/Resources/Photos/{DateTime.UtcNow:yyyyMM}";

            await Parallel.ForEachAsync(files, CancellationToken, async (file, CancellationToken) =>
            {
                try
                {
                    var filePath = $"{path}/a{id.ToString().PadLeft(8, '0')}.{Path.GetRandomFileName()}{Path.GetExtension(file.FileName)}";

                    var targetSize = new Size(width, height);
                    await using var resizedStream = SharedUtils.GetStream();

                    var (_format, size) = await ImageSharpUtils.ResizeImageStreamAsync(file.OpenReadStream(), targetSize, resizedStream, null, CancellationToken);

                    resizedStream.Seek(0, SeekOrigin.Begin);
                    var saveResult = await storage.WriteAsync(filePath, resizedStream, WriteCase.CreateNew);

                    if (saveResult)
                    {
                        // New avatar URL
                        var url = storage.GetUrl(filePath);

                        var data = new GalleryPhotoDto { Url = url, Width = size.Width, Height = size.Height };

                        photos.Enqueue(new GalleryPhotoExtendedDto(data, file.ContentType) { FileSize = file.Length });
                    }
                    else
                    {
                        exceptions.Enqueue(new Exception($"Failed to save photo file {file.FileName}"));
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });

            if (!photos.IsEmpty)
            {
                await Repo.UploadPhotosAsync(id, photos.Select(photo => photo as GalleryPhotoDto));
            }

            ActionResult result;
            if (!exceptions.IsEmpty)
            {
                result = LogException(new AggregateException(exceptions));
            }
            else
            {
                await OnDemandRevalidateAsync(id);
                result = ActionResult.Success;
            }

            await Repo.AddAuditAsync(AuditKind.UpdateGallery, id.ToString(), $"Update article {id} gallery", ip, result, photos);

            return result;
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(int id, HttpResponse response)
        {
            await Repo.UpdateReadAsync(response, id);
        }

        /// <summary>
        /// Read for gallery photos
        /// 阅读图库照片
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task ViewGalleryAsync(int id, HttpResponse response)
        {
            await Repo.ViewGalleryAsync(response, id);
        }

        /// <summary>
        /// Read for view
        /// 阅读浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task ViewReadAsync(int id, HttpResponse response)
        {
            await Repo.ViewReadAsync(response, id);
        }
    }
}