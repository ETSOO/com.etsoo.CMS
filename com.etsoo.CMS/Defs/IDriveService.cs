using com.etsoo.CMS.RQ.Drive;
using com.etsoo.CoreFramework.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Online drive service interface
    /// 网络硬盘服务接口
    /// </summary>
    public interface IDriveService : IServiceBase
    {
        /// <summary>
        /// Delete file
        /// 删除文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <param name="ip">IP address</param>
        /// <returns>Action result</returns>
        ValueTask<IActionResult> DeleteAsync(string id, IPAddress ip);

        /// <summary>
        /// Download file
        /// 下载文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Result</returns>
        ValueTask<(Stream data, string fileName, string contentType)?> DownloadFileAsync(string id);

        /// <summary>
        /// Query files
        /// 查询文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task QueryAsync(DriveQueryRQ rq, HttpResponse response);

        /// <summary>
        /// Share file
        /// 分享文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP</param>
        /// <returns>Result</returns>
        Task<IActionResult> ShareFileAsync(DriveShareFileRQ rq, IPAddress ip);

        /// <summary>
        /// Upload files
        /// 上传文件
        /// </summary>
        /// <param name="files">Files</param>
        /// <param name="ip">IP</param>
        /// <returns>Result</returns>
        /// <exception cref="InvalidDataException"></exception>
        ValueTask<IActionResult> UploadFilesAsync(IEnumerable<IFormFile> files, IPAddress ip);

        /// <summary>
        /// Remove file share
        /// 移除文件分享
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Action result</returns>
        ValueTask<IActionResult> RemoveShareAsync(string id);

        /// <summary>
        /// Update file
        /// 更新文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> UpdateAsync(DriveUpdateRQ rq, IPAddress ip);
    }
}
