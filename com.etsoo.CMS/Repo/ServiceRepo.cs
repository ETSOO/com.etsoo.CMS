using com.etsoo.CMS.Application;
using com.etsoo.CMS.RQ.Service;
using com.etsoo.Database;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Service repository
    /// 服务仓库
    /// </summary>
    public class ServiceRepo : CommonRepo
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        public ServiceRepo(IMyApp app)
            : base(app, "service")
        {

        }

        /// <summary>
        /// Get article
        /// 获取文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response"></param>
        /// <returns>Result</returns>
        public async Task GetArticleAsync(GetArticleRQ rq, HttpResponse response)
        {
            if (rq.Id == null && rq.Tab == null) return;

            var parameters = FormatParameters(rq);

            var fields = "a.id, a.title, a.subtitle, a.description, a.url, a.logo, a.release, a.year, t.name AS tabName, t.layout AS tabLayout, t.url AS tabUrl";
            if (rq.WithContent.GetValueOrDefault()) fields += ", a.content, a.keywords";

            var json = fields.ToJsonCommand(true);
            var command = CreateCommand(@$"SELECT {json} FROM articles AS a
                INNER JOIN tabs AS t ON a.tab1 = t.id
                WHERE a.status < 200
                    AND (@{nameof(rq.Id)} IS NULL OR a.id = @{nameof(rq.Id)})
                    AND (@{nameof(rq.Tab)} IS NULL OR a.tab1 = @{nameof(rq.Tab)})
                    AND (@{nameof(rq.Url)} IS NULL OR a.url = @{nameof(rq.Url)})
                LIMIT 1", parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Get slideshow articles
        /// 获取幻灯片文章
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Result</returns>
        public async Task GetSlideshowsAsync(HttpResponse response)
        {
            var json = "a.id, a.title, a.subtitle, a.description, a.url, a.slideshow, a.year, t.name AS tabName, t.layout AS tabLayout, t.url AS tabUrl".ToJsonCommand();
            var command = CreateCommand(@$"SELECT {json} FROM articles AS a
                INNER JOIN tabs AS t ON a.tab1 = t.id
                WHERE a.status < 200 AND a.slideshow IS NOT NULL ORDER BY t.orderIndex ASC, t.name ASC, a.release DESC");
            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Get website data
        /// 获取网站数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task GetSiteDataAsync(HttpResponse response)
        {
            var site = "title, keywords, description".ToJsonCommand(true);
            var tabs = "id, parent, name, description, logo, layout, url, jsonData";
            var tabsJson = tabs.ToJsonCommand();
            var resouces = "id, value".ToJsonCommand();
            var services = "id, app".ToJsonCommand();
            var command = CreateCommand(@$"SELECT {site} FROM website;
                SELECT {tabsJson} FROM (SELECT {tabs} FROM tabs WHERE status < 200 ORDER BY parent ASC, orderIndex ASC, name ASC);
                SELECT {resouces} FROM resources;
                SELECT {services} FROM services WHERE status < 200;
            ");

            await ReadJsonToStreamAsync(command, response, new[] { "site", "tabs", "resources", "services" });
        }
    }
}
