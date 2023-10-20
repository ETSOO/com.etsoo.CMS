using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.DB;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using System.Text.Json;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Website article repository
    /// 网站文章仓库
    /// </summary>
    public class ArticleRepo : CommonRepo
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">User</param>
        public ArticleRepo(IMyApp app, IServiceUser? user)
            : base(app, "article", user)
        {

        }

        /// <summary>
        /// Create article
        /// 创建文章
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Action result</returns>
        public async Task<ActionDataResult<int>> CreateAsync(ArticleCreateRQ model)
        {
            model.Url = model.Url.Trim('/');

            var parameters = FormatParameters(model);

            var releaseStr = model.Release?.ToUniversalTime().ToString("u");
            parameters.Add(nameof(releaseStr), releaseStr);

            var now = DateTime.UtcNow.ToString("u");
            parameters.Add(nameof(now), now);

            var year = DateTime.UtcNow.Year;
            parameters.Add(nameof(year), year);

            AddSystemParameters(parameters);

            var command = CreateCommand(@$"INSERT INTO articles (title, subtitle, keywords, description, url, content, logo, jsonData, tab1, weight, slideshow, year, creation, release, refreshTime, author, status, orderIndex)
                VALUES (@{nameof(model.Title)},
                    @{nameof(model.Subtitle)},
                    @{nameof(model.Keywords)},
                    @{nameof(model.Description)},
                    @{nameof(model.Url)},
                    @{nameof(model.Content)},
                    @{nameof(model.Logo)},
                    @{nameof(model.JsonData)},
                    @{nameof(model.Tab1)}, @{nameof(model.Weight)},
                    @{nameof(model.Slideshow)},
                    @{nameof(year)}, @{nameof(now)}, @{nameof(releaseStr)}, @{nameof(now)}, {SysUserField}, @{nameof(model.Status)}, 0); SELECT last_insert_rowid();", parameters);

            var articleId = await ExecuteScalarAsync<int>(command);

            return new ActionDataResult<int>(ActionResult.Success, articleId);
        }

        /// <summary>
        /// Delete article
        /// 删除文章
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM articles WHERE id = @{nameof(id)} AND status = 255", parameters);

            var result = await ExecuteAsync(command);

            if (result > 0)
                return ActionResult.Success;
            else
                return ApplicationErrors.NoId.AsResult();
        }

        /// <summary>
        /// Delete photo
        /// 删除照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="url">Photo URL</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> DeletePhotoAsync(int id, string url)
        {
            var items = await ViewGalleryItemsAsync(id);
            if (items?.Any() is true)
            {
                var list = items.ToList();
                var item = list.Find(item => item.Url.Equals(url));
                if (item == null)
                {
                    return ApplicationErrors.NoId.AsResult("url");
                }
                else
                {
                    list.Remove(item);

                    await SavePhotosAsync(id, list);

                    return ActionResult.Success;
                }
            }
            else
            {
                return ApplicationErrors.NoId.AsResult();
            }
        }

        /// <summary>
        /// Query
        /// 查询
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        public async Task<DbArticleQuery[]> QueryAsync(ArticleQueryRQ rq)
        {
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var fields = $"a.id, a.title, IIF(a.author = {SysUserField}, true, false) AS isSelf, a.creation, a.url, t.url AS tabUrl, t.layout AS tabLayout, a.year, a.tab1, a.tab2, a.tab3, a.logo";

            var items = new List<string>();
            if (rq.Id is not null) items.Add($"a.id = @{nameof(rq.Id)}");
            if (rq.Tab is not null) items.Add($"(a.tab1 = @{nameof(rq.Tab)} OR a.tab2 = @{nameof(rq.Tab)} OR a.tab3 = @{nameof(rq.Tab)})");
            if (!string.IsNullOrEmpty(rq.Title)) items.Add($"a.title LIKE '%' || @{nameof(rq.Title)} || '%'");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.BatchSize, rq.CurrentPage);

            var command = CreateCommand(@$"SELECT * FROM (SELECT {fields} FROM articles AS a INNER JOIN tabs AS t ON a.tab1 = t.id {conditions} {rq.GetOrderCommand()} {limit})", parameters);

            var list = await QueryAsListAsync<DbArticleQuery>(command);
            var tabIds = new List<int>();
            foreach (var a in list)
            {
                tabIds.Add(a.Tab1);
                if (a.Tab2.HasValue) tabIds.Add(a.Tab2.Value);
                if (a.Tab3.HasValue) tabIds.Add(a.Tab3.Value);
            }
            var tabs = await QueryTabsAsync(tabIds);

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
        /// Query history
        /// 查询操作历史
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryHistoryAsync(int id, HttpResponse response)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var fields = "id, kind, title, content, creation, ip, flag";
            var json = fields.ToJsonCommand();

            var command = CreateCommand($"SELECT {json} FROM audits WHERE kind IN (12, 13) AND target = @{nameof(id)} ORDER BY id DESC LIMIT 8)", parameters);

            await ReadJsonToStreamAsync(command, response);
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
        /// Query article link data
        /// 获取文章链接数据
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Result</returns>
        public async ValueTask<ArticleLink?> QueryLinkAsync(int id)
        {
            var command = CreateCommand($"SELECT a.id, a.url, a.year, a.tab1, a.tab2, a.tab3, t.layout AS TabLayout, t.url AS TabUrl FROM articles AS a INNER JOIN tabs AS t ON a.tab1 = t.id WHERE a.id = {id}");
            return await QueryAsAsync<ArticleLink>(command);
        }

        /// <summary>
        /// Query multiple tabs list
        /// 获取多个栏目列表
        /// </summary>
        /// <param name="tabs">Tab ids</param>
        /// <returns>List</returns>
        public async ValueTask<DbTabList[]> QueryTabsAsync(IEnumerable<int> tabs)
        {
            var command = CreateCommand($"WITH ctx(id, name, parent, level) AS (SELECT f.id, f.name, f.parent, 0 FROM tabs AS f WHERE f.id IN ({string.Join(", ", tabs)}) UNION ALL SELECT ctx.id, t.name, t.parent, ctx.level + 1 FROM tabs AS t INNER JOIN ctx ON t.id = ctx.parent WHERE ctx.parent IS NOT NULL) SELECT id, group_concat(name, ' -> ') AS name FROM ctx GROUP BY id ORDER BY level ASC");
            return await QueryAsListAsync<DbTabList>(command);
        }

        /// <summary>
        /// Read logo
        /// 读取照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Result</returns>
        public async Task<string?> ReadLogoAsync(int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var command = CreateCommand(@$"SELECT IFNULL(logo, '') FROM articles WHERE id = @{nameof(id)}", parameters);

            return await ExecuteScalarAsync<string?>(command);
        }

        /// <summary>
        /// Save gallery photos
        /// 读取图库照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="items">Photo items</param>
        /// <returns>Result</returns>
        public async Task<int> SavePhotosAsync(int id, IEnumerable<GalleryPhotoDto> items)
        {
            var json = JsonSerializer.Serialize(items, SharedUtils.JsonDefaultSerializerOptions);

            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);
            parameters.Add(nameof(json), json);

            AddSystemParameters(parameters);

            var command = CreateCommand(@$"UPDATE articles SET slideshow = @{nameof(json)} WHERE id = @{nameof(id)}", parameters);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// Update photo gallery item
        /// 更新图片库项目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdatePhotoAsync(ArticleUpdatePhotoRQ rq)
        {
            var items = await ViewGalleryItemsAsync(rq.Id);
            if (items?.Any() is true)
            {
                var item = items.FirstOrDefault(item => item.Url.Equals(rq.Url));
                if (item == null)
                {
                    return ApplicationErrors.NoId.AsResult("url");
                }
                else
                {
                    // Update
                    item.Title = rq.Title;
                    item.Description = rq.Description;
                    item.Link = rq.Link;

                    await SavePhotosAsync(rq.Id, items);

                    return ActionResult.Success;
                }
            }
            else
            {
                return ApplicationErrors.NoId.AsResult();
            }
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="url">Photo URL</param>
        /// <returns>Result</returns>
        public async Task<int> UpdateLogoAsync(int id, string url)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);
            parameters.Add(nameof(url), url);

            AddSystemParameters(parameters);

            var command = CreateCommand(@$"UPDATE articles SET logo = @{nameof(url)} WHERE id = @{nameof(id)}", parameters);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// Async upload photo files
        /// 异步上传照片文件
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="photos">Photos</param>
        /// <returns>Task</returns>
        public async Task<int> UploadPhotosAsync(int id, IEnumerable<GalleryPhotoDto> photos)
        {
            var items = await ViewGalleryItemsAsync(id) ?? Enumerable.Empty<GalleryPhotoDto>();

            foreach (var photo in photos)
            {
                if (!items.Any(item => item.Url.Equals(photo.Url)))
                {
                    items = items.Append(photo);
                }
            }

            return await SavePhotosAsync(id, items);
        }

        /// <summary>
        /// View update JSON data to HTTP Response
        /// 浏览更新JSON数据到HTTP响应
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        public async Task UpdateReadAsync(HttpResponse response, int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var json = $"id, title, subtitle, keywords, description, url, content, logo, jsonData, tab1, weight, release, status".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM articles WHERE id = @{nameof(id)}", parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// View gallery photo data to HTTP Response
        /// 浏览阅读图库到HTTP响应
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        public async Task ViewGalleryAsync(HttpResponse response, int id)
        {
            // Get raw JSON data
            var raw = await ViewGalleryAsync(id);

            // Content type
            response.JsonContentType();

            if (!string.IsNullOrEmpty(raw))
                await response.WriteAsync(raw, CancellationToken);
        }

        /// <summary>
        /// View gallery photo raw data
        /// 浏览阅读图库原始数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        public async Task<string?> ViewGalleryAsync(int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var command = CreateCommand($"SELECT slideshow FROM articles AS a WHERE a.id = @{nameof(id)}", parameters);

            return await ExecuteScalarAsync<string?>(command);
        }

        /// <summary>
        /// View gallery photo items
        /// 浏览阅读图库项目
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<GalleryPhotoDto>?> ViewGalleryItemsAsync(int id)
        {
            var raw = await ViewGalleryAsync(id);
            if (string.IsNullOrEmpty(raw)) return null;
            return JsonSerializer.Deserialize<IEnumerable<GalleryPhotoDto>>(raw, SharedUtils.JsonDefaultSerializerOptions);
        }

        /// <summary>
        /// View read JSON data to HTTP Response
        /// 浏览阅读JSON数据到HTTP响应
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        public async Task ViewReadAsync(HttpResponse response, int id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);

            AddSystemParameters(parameters);

            var tabsCommand = $"SELECT tab1, tab2, tab3 FROM articles WHERE id = @{nameof(id)}";
            var tabData = await QueryAsAsync<DbArticleTabs>(CreateCommand(tabsCommand, parameters));
            if (tabData == null) return;

            var (tab1, tab2, tab3) = await ReadTabsAsync(tabData);
            parameters.Add(nameof(tab1), tab1);
            parameters.Add(nameof(tab2), tab2);
            parameters.Add(nameof(tab3), tab3);

            var basic = $"a.id, a.title, a.subtitle, a.keywords, a.description, a.url, a.logo, a.creation, a.weight, a.author, a.release, a.status, a.slideshow, a.year, t.layout AS tabLayout, t.url AS tabUrl".ToJsonCommand(true);
            basic = basic.Trim(')') + ", 'tabName1', @tab1, 'tabName2', @tab2, 'tabName3', @tab3)";

            var command = CreateCommand($"SELECT {basic} FROM articles AS a INNER JOIN tabs AS t ON a.tab1 = t.id WHERE a.id = @{nameof(id)}", parameters);

            await ReadJsonToStreamAsync(command, response);
        }
    }
}
