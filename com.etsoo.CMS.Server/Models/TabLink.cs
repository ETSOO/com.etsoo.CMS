using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Tab link data
    /// 栏目链接数据
    /// </summary>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record TabLink
    {
        /// <summary>
        /// Tab id
        /// 栏目编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Layout
        /// 布局
        /// </summary>
        public byte Layout { get; init; }

        /// <summary>
        /// URL
        /// 链接
        /// </summary>
        public required string Url { get; init; }

        /// <summary>
        /// Get URL, keep the same with LocalSite.formatUrl
        /// 获取链接地址
        /// </summary>
        /// <returns>Result</returns>
        public string GetUrl()
        {
            if (Layout == 1) return "#";
            return Url;
        }
    }
}
