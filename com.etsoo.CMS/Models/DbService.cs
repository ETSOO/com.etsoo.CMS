using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Table service item
    /// 服务项目
    /// </summary>
    /// <param name="App">App</param>
    /// <param name="Secret">App secret</param>
    [AutoDataReaderGenerator]
    public partial record DbService(string App, string Secret);
}
