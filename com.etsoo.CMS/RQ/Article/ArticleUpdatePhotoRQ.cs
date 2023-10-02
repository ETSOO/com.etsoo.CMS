using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Update article photo gallery request data
    /// 更新文章照片库请求数据
    /// </summary>
    [AutoToParameters]
    public partial record ArticleUpdatePhotoRQ
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
        [Property(Length = 128)]
        public required string Url { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        [Property(Length = 128)]
        public string? Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        [Property(Length = 1280)]
        public string? Description { get; init; }

        /// <summary>
        /// Link
        /// 链接
        /// </summary>
        [Property(Length = 256)]
        public string? Link { get; init; }
    }
}
