using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Gallery photo data
    /// 图库照片数据
    /// </summary>
    [AutoToParameters]
    public partial record GalleryPhotoDto
    {
        /// <summary>
        /// URL
        /// </summary>
        public required string Url { get; init; }

        /// <summary>
        /// Width in pixel
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// Height in pixel
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Link
        /// 链接
        /// </summary>
        public string? Link { get; set; }
    }
}
