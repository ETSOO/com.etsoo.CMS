namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Sort gallery photos request data
    /// 图库照片排序请求数据
    /// </summary>
    public record ArticleSortPhotosRQ
    {
        /// <summary>
        /// Article id
        /// 文章编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Indices array
        /// 索引数组
        /// </summary>
        public required IEnumerable<int> Ids { get; init; }
    }
}
