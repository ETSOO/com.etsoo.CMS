using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.User
{
    /// <summary>
    /// User query data
    /// 用户查询参数
    /// </summary>
    [SqlSelectCommand("users", NamingPolicy.CamelCase, Database = DatabaseName.SQLite)]
    public partial record UserQueryRQ : QueryRQ
    {
        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole? Role { get; init; }
    }
}
