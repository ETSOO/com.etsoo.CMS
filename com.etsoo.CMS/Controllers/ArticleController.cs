using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Web;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Website article controller
    /// 网站文章控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : SharedController
    {
        // Service
        readonly IArticleService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="service">Service</param>
        public ArticleController(IMyApp app, IHttpContextAccessor httpContextAccessor, IArticleService service)
            : base(app, httpContextAccessor)
        {
            this.service = service;
        }

        /// <summary>
        /// Create article
        /// 创建文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("Create")]
        public async Task Create(ArticleCreateRQ rq)
        {
            var result = await service.CreateAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Query article
        /// 查询文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("Query")]
        public async Task<List<DbArticleQuery>> Query(ArticleQueryRQ rq)
        {
            return await service.QueryAsync(rq);
        }

        /// <summary>
        /// Query history
        /// 查询操作历史
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        [HttpPost("QueryHistory/{id:int}")]
        public async Task QueryHistory(int id)
        {
            await service.QueryHistoryAsync(id, Response);
        }

        /// <summary>
        /// Translate
        /// 翻译
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("Translate")]
        public async Task<string> Translate(TranslateRQ rq)
        {
            return await service.TranslateAsync(rq.Text);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.User | UserRole.Founder | UserRole.Admin)]
        [HttpPut("Update")]
        public async Task Update(ArticleUpdateRQ rq)
        {
            var result = await service.UpdateAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        [HttpGet("UpdateRead/{id:int}")]
        public async Task UpdateRead(int id)
        {
            await service.UpdateReadAsync(id, Response);
        }

        /// <summary>
        /// Read for view
        /// 阅读浏览
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        [HttpGet("ViewRead/{id:int}")]
        public async Task ViewRead(int id)
        {
            await service.ViewReadAsync(id, Response);
        }
    }
}
