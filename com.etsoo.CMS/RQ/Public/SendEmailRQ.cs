using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public required string Recipient { get; init; }

        /// <summary>
        /// Subject
        /// 主题
        /// </summary>
        [Required]
        [MaxLength(256)]
        public required string Subject { get; init; }

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
        [MaxLength(1024)]
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
