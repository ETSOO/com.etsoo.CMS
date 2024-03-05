using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Article query data
    /// 文章查询参数
    /// </summary>
    [AutoToParameters]
    public partial record ArticleQueryRQ : QueryRQ<int>
    {
        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Tab
        /// 栏目
        /// </summary>
        public int? Tab { get; init; }
    }
}
