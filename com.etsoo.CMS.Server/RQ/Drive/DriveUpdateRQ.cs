using com.etsoo.CoreFramework.Models;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Drive
{
    /// <summary>
    /// File update request data
    /// 文件更新请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record DriveUpdateRQ : UpdateModel
    {
        /// <summary>
        /// File name
        /// 文件名
        /// </summary>
        [Property(Length = 256)]
        public string? Name { get; init; }

        /// <summary>
        /// Is shared
        /// 是否公开访问
        /// </summary>
        public bool? Shared { get; init; }

        /// <summary>
        /// Remove file shares
        /// 清除所有分享
        /// </summary>
        public bool? RemoveShare { get; init; }
    }
}
