using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Service
{
    /// <summary>
    /// Get articles request data
    /// 读取文章列表请求数据
    /// </summary>
    [AutoToParameters]
    public partial record GetArticlesRQ
    {
        /// <summary>
        /// Article ids
        /// 文章编号
        /// </summary>
        [Property(Ignore = true)]
        public IEnumerable<int>? Ids { get; init; }

        /// <summary>
        /// Tab id
        /// 栏目编号
        /// </summary>
        public int? Tab { get; init; }

        /// <summary>
        /// Batch size
        /// 批量请求数量
        /// </summary>
        [Range(1, 1000)]
        public ushort? BatchSize { get; init; }

        /// <summary>
        /// With content
        /// 返回内容
        /// </summary>
        public bool? WithContent { get; init; }

        /// <summary>
        /// Last release time
        /// 最后发布时间
        /// </summary>
        public DateTime? LastRelease { get; init; }

        /// <summary>
        /// Last id
        /// 最后编号
        /// </summary>
        public int? LastId { get; init; }
    }
}
