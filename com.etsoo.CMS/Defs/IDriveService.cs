using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Drive;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Online drive service interface
    /// 网络硬盘服务接口
    /// </summary>
    public interface IDriveService : ICommonService
    {
        ValueTask<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken = default);

        ValueTask<(Stream data, string fileName, string contentType)?> DownloadFileAsync(string id, CancellationToken cancellationToken = default);

        Task QueryAsync(DriveQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default);

        Task<DriveFile?> ReadAsync(string id, CancellationToken cancellationToken = default);

        Task<IActionResult> ShareFileAsync(DriveShareFileRQ rq, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> UploadFilesAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> RemoveShareAsync(string id, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdateAsync(DriveUpdateRQ rq, CancellationToken cancellationToken = default);
    }
}
