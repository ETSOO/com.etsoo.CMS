using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Parent tab data
    /// 父栏目数据
    /// </summary>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record ParentTab
    {
        /// <summary>
        /// Tab id
        /// 栏目编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Parent tab id
        /// 父栏目编号
        /// </summary>
        public int Parent { get; init; }

        /// <summary>
        /// Tab name
        /// 栏目名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Tab layout
        /// 栏目布局
        /// </summary>
        public int Layout { get; init; }

        /// <summary>
        /// Tab URL
        /// 栏目链接
        /// </summary>
        public required string Url { get; init; }

        /// <summary>
        /// Level
        /// 水平值
        /// </summary>
        public int Level { get; init; }
    }
}
