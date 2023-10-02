using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.RQ.Service;
using com.etsoo.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Site service controller
    /// 网站服务控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ServiceController : SharedController
    {
        // Service
        readonly IExternalService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="service">Service</param>
        public ServiceController(IMyApp app, IHttpContextAccessor httpContextAccessor, ILogger<AuthController> logger, IExternalService service)
            : base(app, httpContextAccessor)
        {
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN)
            var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization;
            if (authorization.HasValue && AuthenticationHeaderValue.TryParse(authorization.ToString(), out var auth) && auth.Scheme == "NextStatic")
            {
                var token = app.Section.GetValue<string>("NextStaticToken");
                if (token?.Equals(auth.Parameter) == true)
                {
                    this.service = service;
                    return;
                }
            }

            throw new UnauthorizedAccessException();
        }

        /// <summary>
        /// Get article
        /// 获取文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("GetArticle")]
        public async Task GetArticle(GetArticleRQ rq)
        {
            await service.GetArticleAsync(rq, Response);
        }

        /// <summary>
        /// Get articles
        /// 获取文章列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("GetArticles")]
        public async Task GetArticles(GetArticlesRQ rq)
        {
            await service.GetArticlesAsync(rq, Response);
        }

        /// <summary>
        /// Get slideshow articles
        /// 获取幻灯片文章
        /// </summary>
        /// <returns>Task</returns>
        [HttpGet("GetSlideshows")]
        public async Task GetSlideshows()
        {
            await service.GetSlideshowsAsync(Response);
        }

        /// <summary>
        /// Get website data
        /// 获取网站数据
        /// </summary>
        /// <returns>Task</returns>
        [HttpGet("GetSiteData")]
        public async Task GetSiteData()
        {
            await service.GetSiteDataAsync(Response);
        }
    }
}
