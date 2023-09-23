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
    }
}
