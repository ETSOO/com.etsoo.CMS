﻿using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Website
{
    /// <summary>
    /// Initialize website request data
    /// 初始化网站请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record InitializeRQ
    {
        /// <summary>
        /// Domain
        /// 域名
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        [Required]
        public required string Domain { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        [Required]
        public required string Title { get; init; }
    }
}
