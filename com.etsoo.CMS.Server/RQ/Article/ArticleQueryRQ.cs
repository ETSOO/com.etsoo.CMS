using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Article query data
    /// 文章查询参数
    /// </summary>
    [SqlSelectCommand("articles AS a INNER JOIN tabs AS t ON a.tab1 = t.id", NamingPolicy.CamelCase, Database = DatabaseName.SQLite)]
    public partial record ArticleQueryRQ : QueryRQ<int>
    {
        [SqlSelectColumn(Prefix = "a")]
        public new int? Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        [SqlColumn(QuerySign = SqlQuerySign.Like)]
        public string? Title { get; init; }

        /// <summary>
        /// Tab
        /// 栏目
        /// </summary>
        [SqlColumn(ColumnNames = ["tab1", "tab2", "tab3"])]
        public int? Tab { get; init; }
    }
}
