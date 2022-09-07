﻿using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Tab
{
    /// <summary>
    /// Tab update request data
    /// 网址栏目更新请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record TabUpdateRQ : UpdateModel<int>
    {
        /// <summary>
        /// Parent tab
        /// 父栏目
        /// </summary>
        public int? Parent { get; init; }

        /// <summary>
        /// Tab name
        /// 栏目名称
        /// </summary>
        [Property(Length = 64)]
        [StringLength(64)]
        public string? Name { get; init; }

        /// <summary>
        /// URL
        /// 网址
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        public string? Url { get; init; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; init; }

        /// <summary>
        /// Layout
        /// 布局
        /// </summary>
        public byte? Layout { get; set; }
    }
}
