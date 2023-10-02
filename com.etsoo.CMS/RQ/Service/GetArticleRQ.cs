using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Service
{
    /// <summary>
    /// Get article request data
    /// 读取文章请求数据
    /// </summary>
    [AutoToParameters]
    public partial record GetArticleRQ
    {
        /// <summary>
        /// Article id
        /// 文章编号
        /// </summary>
        public int? Id { get; init; }

        /// <summary>
        /// Tab id
        /// 栏目编号
        /// </summary>
        public int? Tab { get; init; }

        /// <summary>
        /// URL
        /// 网址
        /// </summary>
        [Property(Length = 128, IsAnsi = true)]
        [StringLength(128)]
        public string? Url { get; init; }

        /// <summary>
        /// With content
        /// 返回内容
        /// </summary>
        public bool? WithContent { get; init; }
    }
}
