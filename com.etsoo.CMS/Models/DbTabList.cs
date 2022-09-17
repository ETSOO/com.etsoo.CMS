using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Table tab list item
    /// 栏目列表项目
    /// </summary>
    /// <param name="Id">Id</param>
    /// <param name="Name">Name</param>
    [AutoDataReaderGenerator]
    public partial record DbTabList(int Id, string Name);
}
