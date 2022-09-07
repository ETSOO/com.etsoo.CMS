using com.etsoo.CMS.Application;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;

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
            var parameters = FormatParameters(model);

            var releaseStr = model.Release?.ToUniversalTime().ToString("s");
            parameters.Add(nameof(releaseStr), releaseStr);

            var now = DateTime.UtcNow.ToString("s");
            parameters.Add(nameof(now), now);

            var year = DateTime.UtcNow.Year;
            parameters.Add(nameof(year), year);

            AddSystemParameters(parameters);

            var command = CreateCommand(@$"INSERT INTO articles (title, subtitle, keywords, description, url, content, logo, tab1, weight, year, creation, release, refreshTime, author, status, orderIndex)
                VALUES (@{nameof(model.Title)},
                    @{nameof(model.Subtitle)},
                    @{nameof(model.Keywords)},
                    @{nameof(model.Description)},
                    @{nameof(model.Url)},
                    @{nameof(model.Content)},
                    @{nameof(model.Logo)},
                    @{nameof(model.Tab1)}, @{nameof(model.Weight)},
                    @{nameof(year)}, @{nameof(now)}, @{nameof(releaseStr)}, @{nameof(now)}, {SysUserField}, 0, 0); SELECT last_insert_rowid();", parameters);

            var articleId = await ExecuteScalarAsync<int>(command);

            return new ActionDataResult<int>(ActionResult.Success, articleId);
        }

        /// <summary>
        /// Query tab
        /// 查询栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(ArticleQueryRQ rq, HttpResponse response)
        {
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var fields = $"a.id, a.title, {$"a.author = {SysUserField}".ToJsonBool()} AS isSelf, a.creation, t1.name AS tab1, t2.name AS tab2, t3.name AS tab3";
            var outFields = "id, title, isSelf, creation, tab1, tab2, tab3";
            var json = outFields.ToJsonCommand();

            var items = new List<string>();
            if (rq.Tab is not null) items.Add($"(a.tab1 = @{nameof(rq.Tab)} OR a.tab2 = @{nameof(rq.Tab)} OR a.tab3 = @{nameof(rq.Tab)})");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.BatchSize, rq.CurrentPage);

            // Sub-select, otherwise 'order by' fails
            var command = CreateCommand(@$"SELECT {json} FROM (SELECT {fields} FROM articles AS a
                INNER JOIN tabs AS t1 ON a.tab1 = t1.id
                LEFT JOIN tabs AS t2 ON a.tab2 = t2.id
                LEFT JOIN tabs AS t3 ON a.tab3 = t3.id
                {conditions} {rq.GetOrderCommand()} {limit})", parameters);

            await ReadJsonToStreamAsync(command, response);
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

            var json = $"id, title, subtitle, keywords, description, url, content, logo, tab1, weight, release".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM articles WHERE id = @{nameof(id)}", parameters);

            await ReadJsonToStreamAsync(command, response, false);
        }
    }
}
