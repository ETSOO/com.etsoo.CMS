using com.etsoo.CoreFramework.Business;
using com.etsoo.SourceGenerators.Attributes;
using com.etsoo.Utils.String;

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
    public partial record DbArticleQuery(int Id, string Title, bool IsSelf, EntityStatus Status, DateTime Creation, int Tab1, int? Tab2, int? Tab3, string Url, int Year, int TabLayout, string TabUrl, string? Logo)
    {
        /// <summary>
        /// Primary tab
        /// 第一栏目
        /// </summary>
        public string? TabName1 { get; set; }

        /// <summary>
        /// Tab 2 name
        /// 第二栏目
        /// </summary>
        public string? TabName2 { get; set; }

        /// <summary>
        /// Tab 3 name
        /// 第三栏目
        /// </summary>
        public string? TabName3 { get; set; }

        public DbArticleQuery(int id, string title, bool isSelf, int status, string creation, int tab1, int? tab2, int? tab3, string url, int year, int tabLayout, string tabUrl, string? logo)
            : this(id, title, isSelf, (EntityStatus)status, StringUtils.TryParse<DateTime>(creation).GetValueOrDefault(), tab1, tab2, tab3, url, year, tabLayout, tabUrl, logo)
        {
        }
    }
}
