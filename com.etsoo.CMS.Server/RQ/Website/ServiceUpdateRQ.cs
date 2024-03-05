using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Website
{
    /// <summary>
    /// Service update request data
    /// 服务更新请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record ServiceUpdateRQ : UpdateModel
    {
        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        [Required]
        [StringLength(512, MinimumLength = 32)]
        public string DeviceId { get; init; } = null!;

        /// <summary>
        /// App id
        /// 程序编号
        /// </summary>
        [Property(Length = 50)]
        [StringLength(50)]
        [Required]
        public string? App { get; init; }

        /// <summary>
        /// App secret
        /// 程序密码
        /// </summary>
        public string? Secret { get; set; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; init; }
    }
}
