using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Table website data
    /// 网站表数据
    /// </summary>
    /// <param name="RowId">Row id</param>
    /// <param name="Domain">Domain</param>
    /// <param name="Title">Title</param>
    /// <param name="Description">Description</param>
    /// <param name="Keywords">Keywords</param>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    [AutoToJson]
    public partial record DbWebsite(int RowId, string Domain, string Title, string? Description, string? Keywords);
}
