using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website article service
    /// 网站文章业务逻辑服务
    /// </summary>
    public class ArticleService : SqliteService<ArticleRepo>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">Current user</param>
        /// <param name="logger">Logger</param>
        /// <param name="httpClientFactory">HttpClient factory</param>
        public ArticleService(IMyApp app, IServiceUser user, ILogger logger, IHttpClientFactory httpClientFactory)
            : base(app, new ArticleRepo(app, user), logger)
        {
            _httpClientFactory=httpClientFactory;
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
            var result = await Repo.CreateAsync(rq);
            var id = result.Data;
            await Repo.AddAuditAsync(AuditKind.CreateArticle, id.ToString(), $"Create article {id}", ip, result.Result, rq);
            return result.MergeData("id");
        }

        /// <summary>
        /// Query article
        /// 查询文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(ArticleQueryRQ rq, HttpResponse response)
        {
            await Repo.QueryAsync(rq, response);
        }

        /// <summary>
        /// Translate
        /// 翻译
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Result</returns>
        public async Task<string> TranslateAsync(string text)
        {
            using var client = _httpClientFactory.CreateClient("Translation");
            using var response = await client.PostAsync("", JsonContent.Create(new { Text = text, TargetLanguageCode = "en", SourceLanguageCode = "zh" }));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
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
            var refreshTime = DateTime.UtcNow.ToString("s");
            var parameters = new Dictionary<string, object>
            {
                [nameof(refreshTime)] = refreshTime
            };

            var (result, _) = await Repo.InlineUpdateAsync<int, UpdateModel<int>>(
                rq,
                new QuickUpdateConfigs(new[] { "title", "subtitle", "keywords", "description", "url", "content", "logo", "tab1", "weight", "release" })
                {
                    TableName = "articles",
                    IdField = "id"
                },
                $"refreshTime = @{nameof(refreshTime)}", parameters
             );
            await Repo.AddAuditAsync(AuditKind.UpdateArticle, rq.Id.ToString(), $"Update article {rq.Id}", ip, result, rq);
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
    }
}