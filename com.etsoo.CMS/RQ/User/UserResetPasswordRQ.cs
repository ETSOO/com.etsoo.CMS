using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.User
{
    /// <summary>
    /// User reset password request data
    /// 用户重置密码请求数据
    /// </summary>
    [AutoToParameters]
    public partial record UserResetPasswordRQ
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        [Required]
        [StringLength(512, MinimumLength = 32)]
        public required string DeviceId { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        [Required]
        public required string Id { get; init; }
    }
}
