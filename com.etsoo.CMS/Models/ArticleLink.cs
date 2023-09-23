using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Article link data
    /// 文章链接数据
    /// </summary>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record ArticleLink
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
        /// Tab layout
        /// 栏目布局
        /// </summary>
        public int TabLayout { get; init; }

        /// <summary>
        /// Tab URL
        /// 栏目链接
        /// </summary>
        public required string TabUrl { get; init; }

        /// <summary>
        /// Get URL
        /// 获取链接地址
        /// </summary>
        /// <returns>Result</returns>
        public string GetUrl()
        {
            if (TabLayout == 0) return TabUrl;
            else if (TabLayout == 1) return string.Empty;
            else return $"{TabUrl}/${Year}/${Url}";
        }
    }
}
