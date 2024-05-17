using com.etsoo.CoreFramework.Business;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;
using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Tab
{
    /// <summary>
    /// Tab create request data
    /// 网址栏目创建请求数据
    /// </summary>
    [SqlInsertCommand("tabs", NamingPolicy.CamelCase, Database = DatabaseName.SQLite)]
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
        public required string Name { get; init; }

        /// <summary>
        /// URL
        /// 网址
        /// </summary>
        [Property(Length = 128)]
        [StringLength(128)]
        public required string Url { get; set; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled
        {
            set
            {
                if (value.HasValue)
                {
                    Status = value.Value ? EntityStatus.Normal : EntityStatus.Inactivated;
                }
            }
        }

        /// <summary>
        /// Layout
        /// 布局
        /// </summary>
        public byte Layout { get; set; } = 0;

        /// <summary>
        /// Logo
        /// 照片
        /// </summary>
        public string? Logo { get; init; }

        /// <summary>
        /// Icon
        /// 小图标
        /// </summary>
        public string? Icon { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        [Property(Length = 1024)]
        [StringLength(1024)]
        public string? Description { get; init; }

        /// <summary>
        /// JSON Data
        /// </summary>
        [IsJson]
        public string? JsonData { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; } = EntityStatus.Normal;

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime => DateTime.Now;

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public int OrderIndex => 0;

        /// <summary>
        /// Articles
        /// 文章数
        /// </summary>
        public int Articles => 0;
    }
}
