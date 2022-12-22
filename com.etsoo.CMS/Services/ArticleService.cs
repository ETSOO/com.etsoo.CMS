using com.etsoo.ApiProxy.Configs;
using com.etsoo.ApiProxy.Defs;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.DI;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website article service
    /// 网站文章业务逻辑服务
    /// </summary>
    public class ArticleService : CommonService<ArticleRepo>, IArticleService
    {
        private readonly IFireAndForgetService fireService;
        private readonly IBridgeProxy bridgeProxy;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="fireService">Fire and go service</param>
        /// <param name="bridgeProxy">Bridge proxy</param>
        public ArticleService(IMyApp app, IServiceUserAccessor userAccessor, ILogger<ArticleService> logger, IFireAndForgetService fireService, IBridgeProxy bridgeProxy)
            : base(app, new ArticleRepo(app, userAccessor.UserSafe), logger)
        {
            this.fireService = fireService;
            this.bridgeProxy = bridgeProxy;
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

            var result = await Repo.CreateAsync(rq);
            var id = result.Data;
            await Repo.AddAuditAsync(AuditKind.CreateArticle, id.ToString(), $"Create article {id}", ip, result.Result, rq);

            await OnDemandRevalidateAsync(id);

            return result.MergeData("id");
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

            // Website repo
            var websiteRepo = new WebsiteRepo(App, Repo.User);

            // NextJs
            var nextJs = await websiteRepo.ReadServiceAsync(NextJsOptions.Name);
            if (nextJs != null)
            {
                var nextJsAddress = nextJs.App;
                var nextJsToken = App.DecriptData(nextJs.Secret);

                // Fire and forget
                fireService.FireAsync<IHttpClientFactory>(async (factory, logger) =>
                {
                    try
                    {
                        var client = factory.CreateClient();
                        var nextJsApi = new NextJsProxy(client, logger, new NextJsOptions
                        {
                            BaseAddress = nextJsAddress,
                            Token = nextJsToken
                        });
                        var result = await nextJsApi.OnDemandRevalidateAsync(url);
                        if (!result.Ok)
                        {
                            logger.LogError("NextJs On-demand revalidataion failed: {@result}", result);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Next.js On-demand Revalidation failed / 按需重新验证失败");
                    }
                });
            }
        }

        /// <summary>
        /// Query article
        /// 查询文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        public async Task<List<DbArticleQuery>> QueryAsync(ArticleQueryRQ rq)
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
                new QuickUpdateConfigs(new[] { "title", "subtitle", "keywords", "description", "url", "content", "logo", "release", "tab1", "weight", "status", "slideshow" })
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