using com.etsoo.CMS.RQ.Public;
using com.etsoo.Utils.Actions;
using com.etsoo.WeiXin.Dto;
using com.etsoo.WeiXin.RQ;

namespace com.etsoo.CMS.Defs
{
    public interface IPublicService
    {
        Task<WXJsApiSignatureResult> CreateJsApiSignatureAsync(CreateJsApiSignatureRQ rq);
        Task<IActionResult> SendEmailAsync(SendEmailRQ rq);
    }
}