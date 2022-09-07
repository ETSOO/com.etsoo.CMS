﻿using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Article create request data
    /// 文章创建请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record ArticleCreateRQ
    {
        /// <summary>
        /// Tab 1
        /// 栏目1，主栏目
        /// </summary>
        [Required]
        public int Tab1 { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        [Property(Length = 256)]
        [StringLength(256)]
        [Required]
        public string Title { get; init; } = default!;

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
        [Property(Length = 512)]
        [StringLength(512)]
        public string? Description { get; init; }

        /// <summary>
        /// URL
        /// 网址
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        [Required]
        public string Url { get; init; } = default!;

        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        [Required]
        public string Content { get; init; } = default!;

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
        public int Weight { get; init; } = 0;
    }
}
