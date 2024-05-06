using com.etsoo.CMS.Defs;
using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.User
{
    /// <summary>
    /// User audits query data
    /// 用户操作历史查询参数
    /// </summary>
    [SqlSelectCommand("audits", NamingPolicy.SnakeCase, Database = DatabaseName.SQLite)]
    public partial record UserHistoryQueryRQ : QueryRQ<int>
    {
        /// <summary>
        /// Author
        /// 作者
        /// </summary>
        public required string Author { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public AuditKind? Kind { get; init; }

        /// <summary>
        /// Creation start
        /// 登记开始时间
        /// </summary>
        public DateTime? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记结束时间
        /// </summary>
        public DateTime? CreationEnd { get; init; }
    }
}
