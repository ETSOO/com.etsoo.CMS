using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.Drive
{
    /// <summary>
    /// Drive file sharing request data
    /// 网络硬盘文件分享请求数据
    /// </summary>
    public partial record DriveShareFileRQ
    {
        /// <summary>
        /// File id
        /// 文件编号
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Hours
        /// 小时数
        /// </summary>
        [Range(1, 720)]
        public int? Hours { get; init; }
    }
}
