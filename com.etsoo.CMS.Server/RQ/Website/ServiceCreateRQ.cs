using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Website
{
    /// <summary>
    /// Service create request data
    /// 服务创建请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record ServiceCreateRQ
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        [Required]
        [StringLength(512, MinimumLength = 32)]
        public required string DeviceId { get; init; }

        /// <summary>
        /// Plugin id
        /// 插件编号
        /// </summary>
        [Property(Length = 30)]
        [StringLength(30)]
        [Required]
        public required string Id { get; init; }

        /// <summary>
        /// App id
        /// 程序编号
        /// </summary>
        [Property(Length = 50)]
        [StringLength(50)]
        [Required]
        public required string App { get; init; }

        /// <summary>
        /// App secret
        /// 程序密码
        /// </summary>
        [Property(Length = 1024)]
        [StringLength(1024)]
        public string? Secret { get; set; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; init; }
    }
}
