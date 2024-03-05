using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.RQ.Drive
{
    /// <summary>
    /// Online drive file create request data
    /// 网络硬盘文件创建请求数据
    /// </summary>
    [AutoToParameters]
    [AutoToJson]
    public partial record DriveCreateRQ
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        [Property(Length = 30, IsAnsi = true)]
        public required string Id { get; init; }

        /// <summary>
        /// File name
        /// 文件名
        /// </summary>
        [Property(Length = 256)]
        public required string Name { get; init; }

        /// <summary>
        /// File path
        /// 文件路径
        /// </summary>
        [Property(Length = 512)]
        public required string Path { get; set; }

        /// <summary>
        /// File size
        /// 文件大小
        /// </summary>
        public required long Size { get; init; }

        /// <summary>
        /// Content type
        /// 文件类型
        /// </summary>
        [Property(Length = 128)]
        public required string ContentType { get; init; }
    }
}
