using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.RQ.Service;
using com.etsoo.Database;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// External service
    /// 外部服务
    /// </summary>
    public class ExternalService : CommonService, IExternalService
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="logger">Logger</param>
        public ExternalService(IMyApp app, ILogger<ExternalService> logger)
            : base(app, null, "service", logger)
        {
        }

        /// <summary>
        /// Get article
        /// 获取文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">HTTP response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task GetArticleAsync(GetArticleRQ rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            if (rq.Id == null && rq.Tab == null) return;

            var parameters = FormatParameters(rq);

            var fields = "a.id, a.title, a.subtitle, a.description, a.jsonData, a.url, a.logo, a.release, a.year, t.name AS tabName, t.layout AS tabLayout, t.url AS tabUrl";
            if (rq.WithContent.GetValueOrDefault()) fields += ", a.content, a.keywords, json(a.slideshow) AS photos";

            var json = fields.ToJsonCommand(true);
            var command = CreateCommand(@$"SELECT {json} FROM articles AS a
                INNER JOIN tabs AS t ON a.tab1 = t.id
                WHERE a.status < 200
                    AND (@{nameof(rq.Id)} IS NULL OR a.id = @{nameof(rq.Id)})
                    AND (@{nameof(rq.Tab)} IS NULL OR a.tab1 = @{nameof(rq.Tab)})
                    AND (@{nameof(rq.Url)} IS NULL OR a.url = @{nameof(rq.Url)})
                LIMIT 1", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Get articles
        /// 获取文章列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">HTTP response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task GetArticlesAsync(GetArticlesRQ rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = FormatParameters(rq);

            var fields = "a.id, a.title, a.subtitle, a.description, a.jsonData, a.url, a.logo, a.release, a.year, t.name AS tabName, t.layout AS tabLayout, t.url AS tabUrl";
            if (rq.WithContent.GetValueOrDefault()) fields += ", a.content, a.keywords, a.slideshow";

            var jsonFields = "id, title, subtitle, description, jsonData, url, logo, release, year, tabName, tabLayout, tabUrl";
            if (rq.WithContent.GetValueOrDefault()) jsonFields += ", content, keywords, json(slideshow) AS photos";

            var json = jsonFields.ToJsonCommand();

            var sql = @$"SELECT
                {json}
            FROM (SELECT {fields} FROM articles AS a
                INNER JOIN tabs AS t ON a.tab1 = t.id
                WHERE a.status < 200
                    AND (@{nameof(rq.Tab)} IS NULL OR a.tab1 = @{nameof(rq.Tab)})
                    {(rq.Ids == null ? string.Empty : $" AND a.id IN ({string.Join(", ", rq.Ids)})")}
                    {(rq.LastRelease.HasValue && rq.LastId.HasValue ? $" AND (a.release, a.id) > (@{nameof(rq.LastRelease)}, @{nameof(rq.LastId)})" : string.Empty)}
                ORDER BY a.release DESC, a.id DESC
                LIMIT {rq.BatchSize ?? 16})";

            var command = CreateCommand(sql, parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Get slideshow articles
        /// 获取幻灯片文章
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task GetSlideshowsAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            var json = "a.id, a.title, a.subtitle, a.description, a.url, json(a.slideshow) as photos, a.year, t.name AS tabName, t.layout AS tabLayout, t.url AS tabUrl".ToJsonCommand();
            var command = CreateCommand(@$"SELECT {json} FROM articles AS a
                INNER JOIN tabs AS t ON a.tab1 = t.id
                WHERE a.status = 9 AND a.slideshow IS NOT NULL ORDER BY t.orderIndex ASC, t.name ASC, a.release DESC", cancellationToken: cancellationToken);
            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Get website data
        /// 获取网站数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task GetSiteDataAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            var site = "title, keywords, description".ToJsonCommand(true);
            var tabs = "id, parent, name, description, logo, icon, layout, url, jsonData";
            var tabsJson = tabs.ToJsonCommand();
            var resouces = "id, value".ToJsonCommand();
            var services = "id, app".ToJsonCommand();
            var command = CreateCommand(@$"SELECT {site} FROM website;
                SELECT {tabsJson} FROM (SELECT {tabs} FROM tabs WHERE status < 200 ORDER BY parent ASC, orderIndex ASC, name ASC);
                SELECT {resouces} FROM resources;
                SELECT {services} FROM services WHERE status < 200;
            ", cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response, ["site", "tabs", "resources", "services"]);
        }
    }
}
