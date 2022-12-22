using com.etsoo.Utils.Serialization;
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
        [PII]
        public string Recipient { get; init; } = default!;

        /// <summary>
        /// Subject
        /// 主题
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Subject { get; init; } = default!;

        /// <summary>
        /// Template name
        /// 模板名称
        /// </summary>
        [Required]
        public string Template { get; init; } = default!;

        /// <summary>
        /// Token
        /// 令牌
        /// </summary>
        [Required]
        [MaxLength(1024)]
        public string Token { get; init; } = default!;

        /// <summary>
        /// Data
        /// 数据
        /// </summary>
        [Required]
        [IsJson]
        public string Data { get; init; } = default!;
    }
}
