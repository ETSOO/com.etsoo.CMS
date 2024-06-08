using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.WeiXin.Dto;
using com.etsoo.WeiXin.RQ;

namespace com.etsoo.CMS.Defs
{
    public interface IPublicService
    {
        Task<WXJsApiSignatureResult> CreateJsApiSignatureAsync(CreateJsApiSignatureRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> SendEmailAsync(SendEmailRQ rq, CancellationToken cancellationToken = default);
    }
}