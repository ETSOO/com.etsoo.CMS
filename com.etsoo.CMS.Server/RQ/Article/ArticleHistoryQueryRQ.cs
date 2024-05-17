using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Server.RQ.Article
{
    /// <summary>
    /// Article audits query data
    /// 文章操作历史查询参数
    /// </summary>
    [SqlSelectCommand("audits", NamingPolicy.CamelCase, Database = DatabaseName.SQLite)]
    public partial record ArticleHistoryQueryRQ : QueryRQ<int>
    {
        /// <summary>
        /// Article id
        /// 文章编号
        /// </summary>
        public required int Target { get; init; }

        /// <summary>
        /// Creation start
        /// 登记开始时间
        /// </summary>
        [SqlColumn(ColumnName = "creation", QuerySign = SqlQuerySign.GreaterOrEqual)]
        public DateTime? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记结束时间
        /// </summary>
        [SqlColumn(ColumnName = "creation", QuerySign = SqlQuerySign.Less)]
        public DateTime? CreationEnd { get; init; }
    }
}
