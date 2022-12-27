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

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="service">Service</param>
        public PublicController(IPublicService service)
        {
            this.service = service;
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
            return new JsonResult(await service.CreateJsApiSignatureAsync(rq));
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
            return new JsonResult(await service.SendEmailAsync(rq));
        }
    }
}
