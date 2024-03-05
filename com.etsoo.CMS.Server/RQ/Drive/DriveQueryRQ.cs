using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Drive
{
    /// <summary>
    /// Online drive query data
    /// 网络硬盘查询参数
    /// </summary>
    [AutoToParameters]
    public partial record DriveQueryRQ : QueryRQ
    {
        /// <summary>
        /// File name
        /// 文件名
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Author
        /// 作者
        /// </summary>
        public string? Author { get; init; }

        /// <summary>
        /// Is shared
        /// 是否公开访问
        /// </summary>
        public bool? Shared { get; init; }

        /// <summary>
        /// Creation start
        /// 创建开始时间
        /// </summary>
        public DateTime? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 创建结束时间
        /// </summary>
        public DateTime? CreationEnd { get; init; }
    }
}
