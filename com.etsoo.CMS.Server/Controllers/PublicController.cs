using com.etsoo.CMS.Defs;
using com.etsoo.CMS.RQ.Public;
using com.etsoo.WeiXin.RQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Public service controller
    /// 公共服务控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly IPublicService service;
        private readonly CancellationToken CancellationToken;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        public PublicController(IPublicService service, IHttpContextAccessor httpContextAccessor)
        {
            this.service = service;
            CancellationToken = httpContextAccessor.HttpContext?.RequestAborted ?? default;
        }

        /// <summary>
        /// Create WeiXin JS siganture
        /// 创建微信 Js 接口签名
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Json data</returns>
        [HttpPut("CreateJsApiSignature")]
        public async Task<IActionResult> CreateJsApiSignature(CreateJsApiSignatureRQ rq)
        {
            return new JsonResult(await service.CreateJsApiSignatureAsync(rq, CancellationToken));
        }

        /// <summary>
        /// Send email
        /// 发送邮件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail(SendEmailRQ rq)
        {
            return new JsonResult(await service.SendEmailAsync(rq, CancellationToken));
        }
    }
}
