using com.etsoo.CoreFramework.Business;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Table article tabs
    /// 文章表格栏目
    /// </summary>
    [AutoDataReaderGenerator]
    public partial record DbArticleTabs
    {
        /// <summary>
        /// Tab 1
        /// 主栏目
        /// </summary>
        public int Tab1 { get; init; }

        /// <summary>
        /// Tab 2
        /// 栏目2
        /// </summary>
        public int? Tab2 { get; init; }

        /// <summary>
        /// Tab 3
        /// 栏目3
        /// </summary>
        public int? Tab3 { get; init; }
    }

    /// <summary>
    /// Table article query data
    /// 文章表查询数据
    /// </summary>
    /// <param name="Id">Id</param>
    /// <param name="Title">Title</param>
    /// <param name="IsSelf">Current user is the author</param>
    /// <param name="Status">Status</param>
    /// <param name="Creation">Creation</param>
    /// <param name="Tab1">Tab 1</param>
    /// <param name="Tab2">Tab 2</param>
    /// <param name="Tab3">Tab 3</param>
    /// <param name="Url">Article URL</param>
    /// <param name="Year">Article year</param>
    /// <param name="Layout">Tab layout</param>
    /// <param name="TabUrl">Tab URL</param>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record DbArticleQuery
    {
        /// <summary>
        /// Article id
        /// 文章编号
        /// </summary>
        [SqlSelectColumn(Prefix = "a")]
        public required int Id { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Author is self or not
        /// 作者是否自己
        /// </summary>
        [SqlSelectColumn(Function = "IIF(a.author = @CurrentUser, true, false)")]
        public bool IsSelf { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTime Creation { get; init; }

        /// <summary>
        /// Primary tab id
        /// 主栏目编号
        /// </summary>
        public int Tab1 { get; init; }

        /// <summary>
        /// Tab 2 id    
        /// 第二栏目编号
        /// </summary>
        public int? Tab2 { get; init; }

        /// <summary>
        /// Tab 3 id
        /// 第三栏目编号
        /// </summary>
        public int? Tab3 { get; init; }

        /// <summary>
        /// Article URL
        /// 文章网址
        /// </summary>
        public required string Url { get; init; }

        /// <summary>
        /// Article release year
        /// 文章发布年份
        /// </summary>
        public int Year { get; init; }

        /// <summary>
        /// Article logo
        /// </summary>
        public string? Logo { get; init; }

        /// <summary>
        /// Tab layout
        /// 栏目布局
        /// </summary>
        [SqlSelectColumn(Prefix = "t")]
        [SqlColumn(ColumnName = "layout")]
        public int TabLayout { get; init; }

        /// <summary>
        /// Tab URL
        /// 栏目网址
        /// </summary>
        [SqlColumn(ColumnName = "url")]
        public required string TabUrl { get; init; }

        /// <summary>
        /// Primary tab
        /// 第一栏目
        /// </summary>
        [SqlColumn(Ignore = true)]
        public string? TabName1 { get; set; }

        /// <summary>
        /// Tab 2 name
        /// 第二栏目
        /// </summary>
        [SqlColumn(Ignore = true)]
        public string? TabName2 { get; set; }

        /// <summary>
        /// Tab 3 name
        /// 第三栏目
        /// </summary>
        [SqlColumn(Ignore = true)]
        public string? TabName3 { get; set; }
    }
}
