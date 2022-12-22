using com.etsoo.CoreFramework.Authentication;
using com.etsoo.SourceGenerators.Attributes;

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
        public required string Id { get; set; }

        /// <summary>
        /// Role
        /// 角色
        /// </summary>
        public required UserRole Role { get; init; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; init; }
    }
}
