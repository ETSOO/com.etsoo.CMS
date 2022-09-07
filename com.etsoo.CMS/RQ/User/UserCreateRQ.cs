using com.etsoo.CoreFramework.Authentication;
using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.User
{
    /// <summary>
    /// User create request data
    /// 用户创建请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record UserCreateRQ
    {
        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        [Property(Length = 128)]
        [Required]
        public string Id { get; init; } = default!;

        /// <summary>
        /// Role
        /// 角色
        /// </summary>
        [Required]
        public UserRole Role { get; init; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; init; }
    }
}
