using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.User
{
    /// <summary>
    /// User update request data
    /// 用户更新请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record UserUpdateRQ : UpdateModel
    {
        /// <summary>
        /// Role
        /// 角色
        /// </summary>
        public UserRole? Role { get; init; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; init; }
    }
}
