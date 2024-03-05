using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;
using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Website
{
    /// <summary>
    /// Website update settings request data
    /// 网站更新设置请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record WebsiteUpdateSettingsRQ : UpdateModel<int>
    {
        /// <summary>
        /// Domain
        /// 域名
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        public string? Domain { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        public string? Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        [Property(Length = 512)]
        [StringLength(512)]
        public string? Description { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        [Property(Length = 512)]
        [StringLength(512)]
        public string? Keywords { get; set; }

        /// <summary>
        /// JSON data
        /// </summary>
        [IsJson]
        public string? JsonData { get; init; }
    }
}
