using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Tab
{
    /// <summary>
    /// Tab create request data
    /// 网址栏目创建请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record TabCreateRQ
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
        [Required]
        public string Name { get; init; } = default!;

        /// <summary>
        /// URL
        /// 网址
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        [Required]
        public string Url { get; init; } = default!;

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// Layout
        /// 布局
        /// </summary>
        public byte Layout { get; set; } = 0;
    }
}
