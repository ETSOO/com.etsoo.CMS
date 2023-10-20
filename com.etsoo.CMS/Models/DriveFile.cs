using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Online drive file
    /// 网络硬盘文件
    /// </summary>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record DriveFile
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Path { get; init; }
        public required string ContentType { get; init; }
        public bool Shared { get; init; }
    }
}
