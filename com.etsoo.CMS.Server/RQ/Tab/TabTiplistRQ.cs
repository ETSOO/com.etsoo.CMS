using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Server.RQ.Tab
{
    [SqlSelectCommand("tabs", NamingPolicy.CamelCase, Database = DatabaseName.SQLite)]
    public partial record TabTiplistRQ : TiplistRQ
    {
        /// <summary>
        /// Tab id
        /// 栏目编号
        /// </summary>
        public int? Id { get; init; }

        /// <summary>
        /// Excluded ids
        /// 排除的编号
        /// </summary>
        [SqlColumn(ColumnName = "id", QuerySign = SqlQuerySign.NotEqual)]
        public IEnumerable<int>? ExcludedIds { get; init; }

        /// <summary>
        /// Keyword
        /// 查询关键词
        /// </summary>
        [SqlColumn(ColumnName = "name", QuerySign = SqlQuerySign.Like)]
        public override string? Keyword { get; set; }
    }
}