using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.SourceGenerators.Attributes;
using com.etsoo.Utils.String;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Table user data
    /// 用户表数据
    /// </summary>
    /// <param name="Id">Username</param>
    /// <param name="Password">Password</param>
    /// <param name="Role">User role</param>
    /// <param name="Status">Status</param>
    /// <param name="Failure">Login failure count</param>
    /// <param name="FrozenTime">Frozen time</param>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record DbUser(string Id, string Password, UserRole Role, EntityStatus Status, int? Failure, DateTime? FrozenTime)
    {
        public DbUser(string id, string password, long role, long status, string? frozenTime)
            : this(id, password, (UserRole)role, (EntityStatus)status, null, StringUtils.TryParse<DateTime>(frozenTime))
        {
        }
    }
}
