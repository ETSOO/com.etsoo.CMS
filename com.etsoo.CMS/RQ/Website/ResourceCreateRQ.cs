using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Website
{
    /// <summary>
    /// Resource create/update request data
    /// 资源创建/修改请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record ResourceCreateRQ
    {
        /// <summary>
        /// Resource id
        /// 资源编号
        /// </summary>
        [Property(Length = 50)]
        [StringLength(50)]
        [Required]
        public string Id { get; init; } = default!;

        /// <summary>
        /// Value
        /// 值
        /// </summary>
        [Required]
        public string Value { get; init; } = default!;
    }
}
