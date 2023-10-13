namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Article link interface
    /// 文章链接接口
    /// </summary>
    public interface IArticleLink
    {
        /// <summary>
        /// Article id
        /// 文章编号
        /// </summary>
        int Id { get; init; }

        /// <summary>
        /// Article URL
        /// 文章链接
        /// </summary>
        string Url { get; init; }

        /// <summary>
        /// Article year
        /// 文章年份
        /// </summary>
        int Year { get; init; }

        /// <summary>
        /// Tab layout
        /// 栏目布局
        /// </summary>
        int TabLayout { get; init; }

        /// <summary>
        /// Tab URL
        /// 栏目链接
        /// </summary>
        string TabUrl { get; init; }
    }

    public static class ArticleLinkExtensions
    {
        /// <summary>
        /// Get tab URL
        /// 获取栏目链接地址
        /// </summary>
        /// <param name="tabLayout">Tab layout</param>
        /// <param name="tabUrl">Tab url</param>
        /// <returns>Result</returns>
        public static string GetTabUrl(int tabLayout, string tabUrl)
        {
            if (tabLayout == 1) return string.Empty;
            else return tabUrl;
        }

        /// <summary>
        /// Get URL
        /// 获取链接地址
        /// </summary>
        /// <param name="link">Article link</param>
        /// <returns>Result</returns>
        public static string GetUrl(this IArticleLink link)
        {
            var tabUrl = GetTabUrl(link.TabLayout, link.TabUrl);

            if (string.IsNullOrEmpty(tabUrl)) return string.Empty;
            else return $"{tabUrl}/{link.Url}";
        }
    }
}
