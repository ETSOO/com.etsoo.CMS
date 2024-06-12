using com.etsoo.ApiProxy.Defs;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CMS.Server;
using com.etsoo.CMS.Server.RQ.Article;
using com.etsoo.CMS.Server.Services;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.DB;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Database;
using com.etsoo.HtmlIO;
using com.etsoo.HTTP;
using com.etsoo.ImageUtils;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using SixLabors.ImageSharp;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website article service
    /// 网站文章业务逻辑服务
    /// </summary>
    public class ArticleService : CommonUserService, IArticleService
    {
        readonly IPAddress ip;
        readonly IBridgeProxy bridgeProxy;
        readonly IStorage storage;
        readonly ITabService tabService;
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
        /// <param name="websiteService">Website service</param>
        /// <param name="tabService">Tab service</param>
        public ArticleService(IMyApp app, IMyUserAccessor userAccessor, ILogger<ArticleService> logger, IBridgeProxy bridgeProxy, IStorage storage, IWebsiteService websiteService, ITabService tabService)
            : base(app, userAccessor.UserSafe, "articles", logger)
        {
            ip = userAccessor.Ip;
            this.bridgeProxy = bridgeProxy;

            // Optional injection
            // IEnumerable<IStorage> storages
            // storage = storages.FirstOrDefault();
            this.storage = storage;

            this.websiteService = websiteService;
            this.tabService = tabService;
        }

        /// <summary>
        /// Create article
        /// 创建文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(ArticleCreateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(rq.Keywords))
            {
                rq.Keywords = ServiceUtils.FormatKeywords(rq.Keywords);
            }
            rq.Url = rq.Url.Trim('/');

            // URL should be unique under the tab
            var parameters = new DbParameters();
            parameters.Add(nameof(rq.Tab1), rq.Tab1);
            parameters.Add(nameof(rq.Url), rq.Url);
            var command = CreateCommand($"SELECT COUNT(*) FROM articles WHERE tab1 = @{nameof(rq.Tab1)} AND url = @{nameof(rq.Url)}", parameters, cancellationToken: cancellationToken);

            var count = await ExecuteScalarAsync<int>(command);
            if (count > 0)
            {
                return ApplicationErrors.ItemExists.AsResult("url");
            }

            rq.Content = await FormatContentAsync(rq.Content, cancellationToken);
            rq.Author = User.Id;

            /*
            var parameters = FormatParameters(rq);

            var releaseStr = rq.Release?.ToUniversalTime().ToString("u");
            parameters.Add(nameof(releaseStr), releaseStr);

            var now = DateTime.UtcNow.ToString("u");
            parameters.Add(nameof(now), now);

            var year = DateTime.UtcNow.Year;
            parameters.Add(nameof(year), year);

            AddSystemParameters(parameters);

            var command = CreateCommand(@$"INSERT INTO articles (title, subtitle, keywords, description, url, content, logo, jsonData, tab1, weight, slideshow, year, creation, release, refreshTime, author, status, orderIndex)
                VALUES (@{nameof(rq.Title)},
                    @{nameof(rq.Subtitle)},
                    @{nameof(rq.Keywords)},
                    @{nameof(rq.Description)},
                    @{nameof(rq.Url)},
                    @{nameof(rq.Content)},
                    @{nameof(rq.Logo)},
                    @{nameof(rq.JsonData)},
                    @{nameof(rq.Tab1)}, @{nameof(rq.Weight)},
                    @{nameof(rq.Slideshow)},
                    @{nameof(year)}, @{nameof(now)}, @{nameof(releaseStr)}, @{nameof(now)}, {SysUserField}, @{nameof(rq.Status)}, 0); SELECT last_insert_rowid();", parameters, cancellationToken: cancellationToken);

            var id = await ExecuteScalarAsync<int>(command);
            */

            var id = await SqlInsertAsync<ArticleCreateRQ, int>(rq, cancellationToken: cancellationToken);

            var result = new ActionDataResult<int>(ActionResult.Success, id);

            var auditTitle = Resources.CreateArticle.Replace("{0}", $"{id} - {rq.Title}");
            await AddAuditAsync(AuditKind.CreateArticle, id.ToString(), auditTitle, ip, result.Result, rq, MyJsonSerializerContext.Default.ArticleCreateRQ, cancellationToken);

            await OnDemandRevalidateAsync(id, cancellationToken);

            return result.Result;
        }

        /// <summary>
        /// Delete article
        /// 删除文章
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            /*
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM articles WHERE id = @{nameof(id)} AND status = 255", parameters, cancellationToken: cancellationToken);

            var result = (await ExecuteAsync(command)) > 0 ? ActionResult.Success : ApplicationErrors.NoId.AsResult();
            */

            var link = await QueryLinkAsync(id, cancellationToken);
            if (link == null) return ApplicationErrors.NoId.AsResult();

            var result = await SqlDeleteAsync<SqlArticleDelete>(new() { Id = id, Status = 255 }, cancellationToken: cancellationToken);

            if (result.Ok)
            {
                await OnDemandRevalidateAsync(link, cancellationToken);
            }

            var auditTitle = Resources.DeleteArticle.Replace("{0}", $"{id}");
            await AddAuditAsync<string?>(AuditKind.DeleteArticle, id.ToString(), auditTitle, ip, result, cancellationToken: cancellationToken);

            return result;
        }

        /// <summary>
        /// Delete photo
        /// 删除照片
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> DeletePhotoAsync(ArticleDeletePhotoRQ rq, CancellationToken cancellationToken = default)
        {
            var id = rq.Id;
            var items = await ViewGalleryItemsAsync(id, cancellationToken);
            IActionResult result;
            if (items?.Any() is true)
            {
                var list = items.ToList();
                var item = list.Find(item => item.Url.Equals(rq.Url));
                if (item == null)
                {
                    result = ApplicationErrors.NoId.AsResult("url");
                }
                else
                {
                    list.Remove(item);

                    await SavePhotosAsync(id, list, cancellationToken);

                    result = ActionResult.Success;
                }
            }
            else
            {
                result = ApplicationErrors.NoId.AsResult();
            }

            if (result.Ok)
            {
                await storage.DeleteUrlAsync(rq.Url);
                await OnDemandRevalidateAsync(rq.Id, cancellationToken);
            }

            var auditTitle = Resources.DeleteArticlePhoto.Replace("{0}", $"{id}");
            await AddAuditAsync(AuditKind.DeleteGalleryPhoto, rq.Id.ToString(), auditTitle, ip, result, rq, MyJsonSerializerContext.Default.ArticleDeletePhotoRQ, cancellationToken);
            return result;
        }

        /// <summary>
        /// On-demand Revalidation
        /// 按需重新验证
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        protected async Task OnDemandRevalidateAsync(int id, CancellationToken cancellationToken = default)
        {
            // Read article link data
            var link = await QueryLinkAsync(id, cancellationToken);
            if (link == null) return;

            await OnDemandRevalidateAsync(link, cancellationToken);
        }

        /// <summary>
        /// On-demand Revalidation
        /// 按需重新验证
        /// </summary>
        /// <param name="link">Article link</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        protected async Task OnDemandRevalidateAsync(ArticleLink link, CancellationToken cancellationToken = default)
        {
            var urls = new List<string>();

            // Article link
            var url = link.GetUrl();
            if (!string.IsNullOrEmpty(url))
            {
                urls.Add(url);
            }

            // Tabs
            var tabIds = new List<int>
            {
                link.Tab1
            };
            if (link.Tab2.HasValue) tabIds.Add(link.Tab2.Value);
            if (link.Tab3.HasValue) tabIds.Add(link.Tab3.Value);

            var tabs = await tabService.AncestorReadAsync(tabIds, cancellationToken);
            foreach (var tab in tabs)
            {
                var tabUrl = ArticleLinkExtensions.GetTabUrl(tab.Layout, tab.Url);
                if (!string.IsNullOrEmpty(tabUrl))
                {
                    urls.Add(tabUrl);
                }
            }

            // Website service
            await websiteService.OnDemandRevalidateAsync(urls, cancellationToken);
        }

        /// <summary>
        /// Query article
        /// 查询文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<DbArticleQuery[]> QueryAsync(ArticleQueryRQ rq, CancellationToken cancellationToken = default)
        {
            /*
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var fields = $"a.id, a.title, IIF(a.author = {SysUserField}, true, false) AS isSelf, a.creation, a.url, t.url AS tabUrl, t.layout AS tabLayout, a.year, a.tab1, a.tab2, a.tab3, a.logo";

            var items = new List<string>();
            if (rq.Id is not null) items.Add($"a.id = @{nameof(rq.Id)}");
            if (rq.Tab is not null) items.Add($"(a.tab1 = @{nameof(rq.Tab)} OR a.tab2 = @{nameof(rq.Tab)} OR a.tab3 = @{nameof(rq.Tab)})");
            if (!string.IsNullOrEmpty(rq.Title)) items.Add($"a.title LIKE '%' || @{nameof(rq.Title)} || '%'");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.QueryPaging);

            var command = CreateCommand(@$"SELECT * FROM (SELECT {fields} FROM articles AS a INNER JOIN tabs AS t ON a.tab1 = t.id {conditions} {rq.QueryPaging.GetOrderCommand()} {limit})", parameters, cancellationToken: cancellationToken);

            var list = await QueryAsListAsync<DbArticleQuery>(command);
            */

            var list = await SqlSelectAsync<ArticleQueryRQ, DbArticleQuery>(rq, true, cancellationToken: cancellationToken);

            var tabIds = new List<int>();
            foreach (var a in list)
            {
                tabIds.Add(a.Tab1);
                if (a.Tab2.HasValue) tabIds.Add(a.Tab2.Value);
                if (a.Tab3.HasValue) tabIds.Add(a.Tab3.Value);
            }
            var tabs = await QueryTabsAsync(tabIds, cancellationToken);

            foreach (var a in list)
            {
                var tab = tabs.FirstOrDefault(tab => tab.Id.Equals(a.Tab1));
                if (tab != null) a.TabName1 = tab.Name;

                if (a.Tab2.HasValue)
                {
                    var tab2 = tabs.FirstOrDefault(tab => tab.Id.Equals(a.Tab2.Value));
                    if (tab2 != null) a.TabName2 = tab2.Name;
                }

                if (a.Tab3.HasValue)
                {
                    var tab3 = tabs.FirstOrDefault(tab => tab.Id.Equals(a.Tab3.Value));
                    if (tab3 != null) a.TabName3 = tab3.Name;
                }
            }

            return list;
        }

        /// <summary>
        /// Query article history
        /// 查询文章操作历史
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public Task HistoryAsync(ArticleHistoryQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            return SqlSelectJsonAsync(rq, ["rowid AS id", "author", "kind", "title", "content", "creation", "ip", "flag"], response, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query multiple tabs list
        /// 获取多个栏目列表
        /// </summary>
        /// <param name="tabs">Tab ids</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List</returns>
        private async ValueTask<DbTabList[]> QueryTabsAsync(IEnumerable<int> tabs, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand($"WITH ctx(id, name, parent, level) AS (SELECT f.id, f.name, f.parent, 0 FROM tabs AS f WHERE f.id IN ({string.Join(", ", tabs)}) UNION ALL SELECT ctx.id, t.name, t.parent, ctx.level + 1 FROM tabs AS t INNER JOIN ctx ON t.id = ctx.parent WHERE ctx.parent IS NOT NULL) SELECT id, group_concat(name, ' -> ') AS name FROM ctx GROUP BY id ORDER BY level ASC", cancellationToken: cancellationToken);
            return await QueryAsListAsync<DbTabList>(command);
        }

        /// <summary>
        /// Query article link data
        /// 获取文章链接数据
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async ValueTask<ArticleLink?> QueryLinkAsync(int id, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand($"SELECT a.id, a.url, a.year, a.tab1, a.tab2, a.tab3, t.layout AS TabLayout, t.url AS TabUrl FROM articles AS a INNER JOIN tabs AS t ON a.tab1 = t.id WHERE a.id = {id}", cancellationToken: cancellationToken);
            return await QueryAsAsync<ArticleLink>(command);
        }

        private async Task<(string? tab1, string? tab2, string? tab3)> ReadTabsAsync(DbArticleTabs a)
        {
            var tabIds = new List<int>
            {
                a.Tab1
            };
            if (a.Tab2.HasValue) tabIds.Add(a.Tab2.Value);
            if (a.Tab3.HasValue) tabIds.Add(a.Tab3.Value);

            var tabs = await QueryTabsAsync(tabIds);
            var tab1 = tabs.FirstOrDefault(tab => tab.Id.Equals(a.Tab1))?.Name;
            var tab2 = tabs.FirstOrDefault(tab => tab.Id.Equals(a.Tab2))?.Name;
            var tab3 = tabs.FirstOrDefault(tab => tab.Id.Equals(a.Tab3))?.Name;
            return (tab1, tab2, tab3);
        }

        /// <summary>
        /// Save gallery photos
        /// 读取图库照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="items">Photo items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async Task<int> SavePhotosAsync(int id, IEnumerable<GalleryPhotoDto> items, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(items, MyJsonSerializerContext.Default.IEnumerableGalleryPhotoDto);

            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);
            parameters.Add(nameof(json), json);

            var command = CreateCommand(@$"UPDATE articles SET slideshow = @{nameof(json)} WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// Sort gallery photos
        /// 图库照片排序
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> SortPhotosAsync(ArticleSortPhotosRQ rq, CancellationToken cancellationToken = default)
        {
            var items = await ViewGalleryItemsAsync(rq.Id, cancellationToken);
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

                    await SavePhotosAsync(rq.Id, mapItems, cancellationToken);

                    await OnDemandRevalidateAsync(rq.Id, cancellationToken);

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

            var auditTitle = Resources.SortArticlePhotos.Replace("{0}", $"{rq.Id}");
            await AddAuditAsync(AuditKind.SortGalleryPhoto, rq.Id.ToString(), auditTitle, ip, result, rq, MyJsonSerializerContext.Default.ArticleSortPhotosRQ, cancellationToken);

            return result;
        }

        /// <summary>
        /// Translate
        /// 翻译
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
        {
            return await bridgeProxy.TranslateTextAsync(new()
            {
                Text = text,
                TargetLanguageCode = "en",
                SourceLanguageCode = "zh"
            }, cancellationToken);
        }

        private async Task<string> FormatContentAsync(string content, CancellationToken cancellationToken = default)
        {
            if (storage == null) return content;

            var path = $"/Resources/{DateTime.UtcNow:yyyyMM}/";
            return await HtmlIOUtils.FormatEditorContentAsync(storage, path, content, Logger, cancellationToken) ?? content;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(ArticleUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(rq.Url))
            {
                rq.Url = rq.Url.Trim('/');

                // Tab
                var tabId = rq.Tab1;
                if (!tabId.HasValue)
                {
                    var tabCommand = CreateCommand($"SELECT tab1 FROM articles WHERE id = {rq.Id}", cancellationToken: cancellationToken);
                    tabId = await ExecuteScalarAsync<int>(tabCommand);
                }

                if (tabId.HasValue)
                {
                    // URL should be unique under the tab
                    var urlParameters = new DbParameters();
                    urlParameters.Add(nameof(rq.Id), rq.Id);
                    urlParameters.Add(nameof(rq.Tab1), tabId);
                    urlParameters.Add(nameof(rq.Url), rq.Url);
                    var command = CreateCommand($"SELECT COUNT(*) FROM articles WHERE tab1 = @{nameof(rq.Tab1)} AND id <> @{nameof(rq.Id)} AND url = @{nameof(rq.Url)}", urlParameters, cancellationToken: cancellationToken);

                    var count = await ExecuteScalarAsync<int>(command);
                    if (count > 0)
                    {
                        return ApplicationErrors.ItemExists.AsResult("url");
                    }
                }
            }

            if (!string.IsNullOrEmpty(rq.Content)
                && rq.ChangedFields?.Contains("content", StringComparer.OrdinalIgnoreCase) is true)
            {
                rq.Content = await FormatContentAsync(rq.Content, cancellationToken);
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

            var (result, _) = await InlineUpdateAsync<int, UpdateModel<int>>(rq, new QuickUpdateConfigs(["title", "subtitle", "keywords", "description", "url", "content", "logo", "jsonData", "release", "tab1", "weight", "status", "slideshow"])
            {
                TableName = "articles",
                IdField = "id"
            }, $"refreshTime = @{nameof(refreshTime)}", parameters,
            cancellationToken);

            var auditTitle = Resources.UpdateArticle.Replace("{0}", $"{rq.Id}");
            await AddAuditAsync(AuditKind.UpdateArticle, rq.Id.ToString(), auditTitle, ip, result, rq, MyJsonSerializerContext.Default.ArticleUpdateRQ, cancellationToken);

            await OnDemandRevalidateAsync(rq.Id, cancellationToken);

            return result;
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="url">Photo URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<int> UpdateLogoAsync(int id, string url, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);
            parameters.Add(nameof(url), url);

            var command = CreateCommand(@$"UPDATE articles SET logo = @{nameof(url)} WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// Update photo gallery item
        /// 更新图片库项目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdatePhotoAsync(ArticleUpdatePhotoRQ rq, CancellationToken cancellationToken = default)
        {
            var items = await ViewGalleryItemsAsync(rq.Id, cancellationToken);
            IActionResult result;
            if (items?.Any() is true)
            {
                var item = items.FirstOrDefault(item => item.Url.Equals(rq.Url));
                if (item == null)
                {
                    result = ApplicationErrors.NoId.AsResult("url");
                }
                else
                {
                    // Update
                    item.Title = rq.Title;
                    item.Description = rq.Description;
                    item.Link = rq.Link;

                    await SavePhotosAsync(rq.Id, items, cancellationToken);

                    result = ActionResult.Success;
                }
            }
            else
            {
                result = ApplicationErrors.NoId.AsResult();
            }

            if (result.Ok && storage != null)
            {
                await OnDemandRevalidateAsync(rq.Id, cancellationToken);
            }

            var auditTitle = Resources.UpdateArticleGalleryItem.Replace("{0}", $"{rq.Id}");
            await AddAuditAsync(AuditKind.UpdateGalleryItem, rq.Id.ToString(), auditTitle, ip, result, rq, MyJsonSerializerContext.Default.ArticleUpdatePhotoRQ, cancellationToken);

            return result;
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="logoStream">Logo stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New URL</returns>
        public async ValueTask<string?> UploadLogoAsync(int id, Stream logoStream, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var command = CreateCommand(@$"SELECT IFNULL(logo, '') FROM articles WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            var logo = await ExecuteScalarAsync<string?>(command);
            if (logo == null)
            {
                return null;
            }

            // File path
            var path = $"/Resources/ArticleLogos/a{id.ToString().PadLeft(8, '0')}.{Path.GetRandomFileName()}{extension}";

            // Save the stream to file directly
            var saveResult = await storage.WriteAsync(path, logoStream, WriteCase.CreateNew, cancellationToken);

            var auditTitle = Resources.UpdateArticleLogo.Replace("{0}", $"{id}");

            if (saveResult)
            {
                // New avatar URL
                var url = storage.GetUrl(path);

                // Repo update
                if (await UpdateLogoAsync(id, url, cancellationToken) > 0)
                {
                    // Audit
                    await AddAuditAsync(AuditKind.UpdateArticleLogo, id.ToString(), auditTitle, new Dictionary<string, object> { ["Logo"] = logo, ["NewLogo"] = url }, null, ip, cancellationToken: cancellationToken);

                    await OnDemandRevalidateAsync(id, cancellationToken);

                    // Return
                    return url;
                }
            }

            Logger.LogError("Storage writing logo failed, the write path is {path}", path);

            await AddAuditAsync(AuditKind.UpdateArticleLogo, id.ToString(), auditTitle, ip, new ActionResult(), path, null, cancellationToken);

            return null;
        }

        /// <summary>
        /// Async upload photo files
        /// 异步上传照片文件
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="photos">Photos</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<int> UploadPhotosAsync(int id, IEnumerable<GalleryPhotoDto> photos, CancellationToken cancellationToken = default)
        {
            var items = await ViewGalleryItemsAsync(id, cancellationToken) ?? [];

            foreach (var photo in photos)
            {
                if (!items.Any(item => item.Url.Equals(photo.Url)))
                {
                    items = items.Append(photo);
                }
            }

            return await SavePhotosAsync(id, items, cancellationToken);
        }

        /// <summary>
        /// Async upload photo files
        /// 异步上传照片文件
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="files">Photo files</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> UploadPhotosAsync(int id, IEnumerable<IFormFile> files, CancellationToken cancellationToken = default)
        {
            var websiteData = await websiteService.ReadJsonDataAsync(MyJsonSerializerContext.Default.JsonDataGalleryLogoSize, cancellationToken);
            var size = websiteData?.GalleryLogoSize;
            var width = size?.FirstOrDefault() ?? 800;
            var height = size?.Count() > 1 ? size.ElementAt(1) : 0;

            var exceptions = new ConcurrentQueue<Exception>();
            var photos = new ConcurrentQueue<GalleryPhotoExtendedDto>();

            // File path
            var path = $"/Resources/Photos/{DateTime.UtcNow:yyyyMM}";

            await Parallel.ForEachAsync(files, cancellationToken, async (file, CancellationToken) =>
            {
                try
                {
                    var filePath = $"{path}/a{id.ToString().PadLeft(8, '0')}.{Path.GetRandomFileName()}{Path.GetExtension(file.FileName)}";

                    var targetSize = new Size(width, height);
                    await using var resizedStream = SharedUtils.GetStream();

                    var (_format, size) = await ImageSharpUtils.ResizeImageStreamAsync(file.OpenReadStream(), targetSize, resizedStream, null, CancellationToken);

                    resizedStream.Seek(0, SeekOrigin.Begin);
                    var saveResult = await storage.WriteAsync(filePath, resizedStream, WriteCase.CreateNew, CancellationToken);

                    if (saveResult)
                    {
                        // New avatar URL
                        var url = storage.GetUrl(filePath);

                        var data = new GalleryPhotoDto { Url = url, Width = size.Width, Height = size.Height };

                        photos.Enqueue(new GalleryPhotoExtendedDto(data, file.ContentType) { FileSize = file.Length });
                    }
                    else
                    {
                        var errorMessage = Resources.UpdateArticleGalleryError.Replace("{0}", file.FileName);
                        exceptions.Enqueue(new Exception(errorMessage));
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });

            var photoItems = photos.Select(photo => photo as GalleryPhotoDto);
            if (photoItems.Any())
            {
                await UploadPhotosAsync(id, photoItems, cancellationToken);
            }

            ActionResult result;
            if (!exceptions.IsEmpty)
            {
                result = LogException(new AggregateException(exceptions));
            }
            else
            {
                await OnDemandRevalidateAsync(id, cancellationToken);
                result = ActionResult.Success;
            }

            var auditTitle = Resources.UpdateArticleGallery.Replace("{0}", $"{id}");
            await AddAuditAsync(AuditKind.UpdateGallery, id.ToString(), auditTitle, ip, result, photoItems, MyJsonSerializerContext.Default.IEnumerableGalleryPhotoDto, cancellationToken);

            return result;
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var json = $"id, title, subtitle, keywords, description, url, content, logo, jsonData, tab1, weight, release, status".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM articles WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Read for gallery photos
        /// 阅读图库照片
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task ViewGalleryAsync(int id, HttpResponse response, CancellationToken cancellationToken = default)
        {
            // Get raw JSON data
            var raw = await ViewGalleryAsync(id, cancellationToken);

            await response.WriteRawJsonAsync(raw, cancellationToken);
        }

        /// <summary>
        /// View gallery photo raw data
        /// 浏览阅读图库原始数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<string?> ViewGalleryAsync(int id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var command = CreateCommand($"SELECT slideshow FROM articles AS a WHERE a.id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            return await ExecuteScalarAsync<string?>(command);
        }

        /// <summary>
        /// View gallery photo items
        /// 浏览阅读图库项目
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<GalleryPhotoDto>?> ViewGalleryItemsAsync(int id, CancellationToken cancellationToken = default)
        {
            var raw = await ViewGalleryAsync(id, cancellationToken);
            if (string.IsNullOrEmpty(raw)) return null;
            return await JsonSerializer.DeserializeAsync(SharedUtils.GetStream(raw), MyJsonSerializerContext.Default.IEnumerableGalleryPhotoDto, cancellationToken);
        }

        /// <summary>
        /// Read for view
        /// 阅读浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task ViewReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            var tabsCommand = $"SELECT tab1, tab2, tab3 FROM articles WHERE id = @{nameof(id)}";
            var tabData = await QueryAsAsync<DbArticleTabs>(CreateCommand(tabsCommand, parameters, cancellationToken: cancellationToken));
            if (tabData == null) return;

            var (tab1, tab2, tab3) = await ReadTabsAsync(tabData);
            parameters.Add(nameof(tab1), tab1);
            parameters.Add(nameof(tab2), tab2);
            parameters.Add(nameof(tab3), tab3);

            var basic = $"a.id, a.title, a.subtitle, a.keywords, a.description, a.url, a.logo, a.creation, a.weight, a.author, a.release, a.status, a.slideshow, a.year, a.jsonData, t.layout AS tabLayout, t.url AS tabUrl".ToJsonCommand(true);
            basic = basic.Trim(')') + ", 'tabName1', @tab1, 'tabName2', @tab2, 'tabName3', @tab3)";

            var audits = $"rowid, title, creation, author".ToJsonCommand();

            // Kinds = UpdateArticle(13), UpdateArticleLogo(16)
            var command = CreateCommand(@$"SELECT {basic} FROM articles AS a INNER JOIN tabs AS t ON a.tab1 = t.id WHERE a.id = @{nameof(id)};
            SELECT {audits} FROM (SELECT rowid, * FROM audits WHERE target = @{nameof(id)} AND kind IN (13, 16) ORDER BY rowid DESC LIMIT 6)
            ", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response, ["data", "audits"]);
        }
    }
}