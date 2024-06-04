using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Public
{
    /// <summary>
    /// Send email request data
    /// 发送邮件请求数据
    /// </summary>
    public class SendEmailRQ
    {
        /// <summary>
        /// Recipient
        /// 收件人
        /// </summary>
        [EmailAddress]
        [Required]
        public required string Recipient { get; init; }

        /// <summary>
        /// Template name
        /// 模板名称
        /// </summary>
        [Required]
        public required string Template { get; init; }

        /// <summary>
        /// Token
        /// 令牌
        /// </summary>
        [Required]
        [MaxLength(2048)]
        public required string Token { get; init; }

        /// <summary>
        /// Data
        /// 数据
        /// </summary>
        [Required]
        [IsJson]
        public required string Data { get; init; }
    }
}
