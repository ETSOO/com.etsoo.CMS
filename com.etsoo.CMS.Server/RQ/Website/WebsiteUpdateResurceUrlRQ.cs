using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Website
{
    [AutoToParameters]
    public partial record WebsiteUpdateResurceUrlRQ
    {
        /// <summary>
        /// Old resource URL
        /// 旧资源路径
        /// </summary>
        public required string OldResourceUrl { get; init; }

        /// <summary>
        /// New resource URL
        /// 新资源路径
        /// </summary>
        public required string ResourceUrl { get; init; }
    }
}
