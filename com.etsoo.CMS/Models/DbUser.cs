using com.etsoo.CoreFramework.Business;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Table user data
    /// 用户表数据
    /// </summary>
    /// <param name="Id">Username</param>
    /// <param name="Password">Password</param>
    /// <param name="Status">Status</param>
    /// <param name="FrozenTime">Frozen time</param>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record DbUser(string Id, string Password, EntityStatus Status, DateTime? FrozenTime);
}
