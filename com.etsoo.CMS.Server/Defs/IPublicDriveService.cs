using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;

namespace com.etsoo.CMS.Server.Defs
{
    public interface IPublicDriveService : ICommonService
    {
        Task<DriveFile?> ReadAsync(string id, CancellationToken cancellationToken = default);
    }
}
