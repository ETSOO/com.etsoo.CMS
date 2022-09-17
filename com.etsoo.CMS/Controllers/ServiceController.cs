using com.etsoo.CMS.Application;
using com.etsoo.CMS.RQ.Service;
using com.etsoo.CMS.Services;
using com.etsoo.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ServiceController : SharedController
    {
        // Service
        readonly ExternalService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="logger">Logger</param>
        public ServiceController(IMyApp app, IHttpContextAccessor httpContextAccessor, ILogger<AuthController> logger)
            : base(app, httpContextAccessor)
        {
            var token = app.Section.GetValue<string>("NextStaticToken");
            var authorization = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            var tokenKey = "NextStatic ";
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(tokenKey) || !token.Equals(authorization[(tokenKey.Length)..]))
            {
                throw new ApplicationException("No Next Static Token");
            }

            service = new ExternalService(app, logger);
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
