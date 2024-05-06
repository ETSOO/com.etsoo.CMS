using com.etsoo.CoreFramework.Business;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;
using com.etsoo.WebUtils.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Article create request data
    /// 文章创建请求数据
    /// </summary>
    [SqlInsertCommand("articles", NamingPolicy.CamelCase, Database = DatabaseName.SQLite)]
    public partial record ArticleCreateRQ
    {
        /// <summary>
        /// Tab 1
        /// 栏目1，主栏目
        /// </summary>
        public required int Tab1 { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        [Property(Length = 256)]
        [StringLength(256)]
        public required string Title { get; init; }

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
        public string? Keywords { get; set; }

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
        public required string Url { get; set; }

        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        [Property(Length = -1)]
        public required string Content { get; set; }

        /// <summary>
        /// Logo
        /// 图标
        /// </summary>
        public string? Logo { get; init; }

        /// <summary>
        /// Release
        /// 发布时间
        /// </summary>
        public DateTimeOffset Release { get; set; } = DateTime.Now;

        /// <summary>
        /// Weight
        /// 权重
        /// </summary>
        public int Weight { get; init; } = 0;

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
        public EntityStatus Status { get; init; } = EntityStatus.Normal;

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public byte OrderIndex => 0;

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation => DateTime.Now;

        /// <summary>
        /// Year
        /// 年份
        /// </summary>
        public int Year => Creation.Year;

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime => DateTime.Now;

        /// <summary>
        /// Author
        /// 作者
        /// </summary>
        [JsonIgnore]
        public string? Author { get; set; }
    }
}
