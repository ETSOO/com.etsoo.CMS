using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Tab
{
    /// <summary>
    /// Tab query data
    /// 栏目查询参数
    /// </summary>
    [SqlSelectCommand("tabs", NamingPolicy.CamelCase, Database = DatabaseName.SQLite)]
    public partial record TabQueryRQ : QueryRQ<int>
    {
        /// <summary>
        /// Parent tab
        /// 父栏目
        /// </summary>
        [SqlColumn(KeepNull = true)]
        public int? Parent { get; init; }

        /// <summary>
        /// Enabled
        /// 是否可用
        /// </summary>
        [SqlColumn(Ignore = true)]
        public bool? Enabled { get; init; }
    }
}
