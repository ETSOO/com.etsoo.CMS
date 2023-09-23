namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Delete article gallery photo request data
    /// 删除文件图库照片请求数据
    /// </summary>
    public record ArticleDeletePhotoRQ
    {
        /// <summary>
        /// Article id
        /// 文章编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Photo URL
        /// 照片地址
        /// </summary>
        public required string Url { get; init; }
    }
}
