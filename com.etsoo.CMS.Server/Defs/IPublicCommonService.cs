using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;

namespace com.etsoo.CMS.Server.Defs
{
    public interface IPublicCommonService : ICommonService
    {
        Task<DriveFile?> ReadDriveAsync(string id, CancellationToken cancellationToken = default);
        Task<string?> QueryResourceAsync(string Id, CancellationToken cancellationToken = default);
        Task<DbService> ReadServiceAsync(string id, CancellationToken cancellationToken = default);
    }
}
