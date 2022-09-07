using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Tab
{
    /// <summary>
    /// Tab query data
    /// 栏目查询参数
    /// </summary>
    [AutoToParameters]
    public partial record TabQueryRQ : QueryRQ<int>
    {
        /// <summary>
        /// Parent tab
        /// 父栏目
        /// </summary>
        public int? Parent { get; init; }

        /// <summary>
        /// Enabled
        /// 是否可用
        /// </summary>
        public bool? Enabled { get; init; }
    }
}
