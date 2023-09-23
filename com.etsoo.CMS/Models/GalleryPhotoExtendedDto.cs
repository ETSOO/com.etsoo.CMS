using System.Diagnostics.CodeAnalysis;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Gallery photo data extended
    /// 图库照片数据扩展
    /// </summary>
    public record GalleryPhotoExtendedDto : GalleryPhotoDto
    {

        /// <summary>
        /// File size
        /// 文件大小
        /// </summary>
        public long FileSize { get; init; }

        /// <summary>
        /// Content type
        /// 内容类型
        /// </summary>
        public required string ContentType { get; init; }

        [SetsRequiredMembers]
        public GalleryPhotoExtendedDto(GalleryPhotoDto data, string contentType) : base(data)
        {
            ContentType = contentType;
        }
    }
}
