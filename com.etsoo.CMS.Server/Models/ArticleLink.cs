using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Article link data
    /// 文章链接数据
    /// </summary>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record ArticleLink : IArticleLink
    {
        /// <summary>
        /// Article id
        /// 文章编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Article URL
        /// 文章链接
        /// </summary>
        public required string Url { get; init; }

        /// <summary>
        /// Article year
        /// 文章年份
        /// </summary>
        public int Year { get; init; }

        /// <summary>
        /// Primary tab id
        /// 主栏目编号
        /// </summary>
        public int Tab1 { get; init; }

        /// <summary>
        /// Tab 2 id
        /// 栏目2编号
        /// </summary>
        public int? Tab2 { get; init; }

        /// <summary>
        /// Tab 3 id
        /// 栏目3编号
        /// </summary>
        public int? Tab3 { get; init; }

        /// <summary>
        /// Tab layout
        /// 栏目布局
        /// </summary>
        public int TabLayout { get; init; }

        /// <summary>
        /// Tab URL
        /// 栏目链接
        /// </summary>
        public required string TabUrl { get; init; }
    }
}
