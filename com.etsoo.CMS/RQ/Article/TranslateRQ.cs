using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Article
{
    /// <summary>
    /// Translate request data
    /// 翻译请求数据
    /// </summary>
    [AutoToParameters]
    public partial record TranslateRQ
    {
        [Required]
        [StringLength(512, MinimumLength = 1)]
        public string Text { get; init; } = null!;
    }
}
