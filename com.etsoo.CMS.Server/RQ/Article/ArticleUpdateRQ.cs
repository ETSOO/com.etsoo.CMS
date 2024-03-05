using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;
using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Article update request data
    /// 文章更新请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record ArticleUpdateRQ : UpdateModel<int>
    {
        /// <summary>
        /// Tab 1
        /// 栏目1，主栏目
        /// </summary>
        public int? Tab1 { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        [Property(Length = 256)]
        [StringLength(256)]
        public string? Title { get; init; }

        /// <summary>
        /// Subtitle
        /// 副标题
        /// </summary>
        [Property(Length = 256)]
        [StringLength(256)]
        public string? Subtitle { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        [Property(Length = 256)]
        [StringLength(256)]
        public string? Keywords { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        [Property(Length = 1024)]
        [StringLength(1024)]
        public string? Description { get; init; }

        /// <summary>
        /// URL
        /// 网址
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        public string? Url { get; set; }

        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        [Property(Length = -1)]
        public string? Content { get; set; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }

        /// <summary>
        /// Release
        /// 发布时间
        /// </summary>
        public DateTime? Release { get; init; }

        /// <summary>
        /// Weight
        /// 权重
        /// </summary>
        public int? Weight { get; init; }

        /// <summary>
        /// Slideshow photo
        /// 幻灯片照片
        /// </summary>
        public string? Slideshow { get; init; }

        /// <summary>
        /// JSON Data
        /// </summary>
        [IsJson]
        public string? JsonData { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus? Status { get; init; }
    }
}
