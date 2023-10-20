using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.RQ.Drive;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Web;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Online drive controller
    /// 网络硬盘控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DriveController : SharedController
    {
        // Service
        readonly IDriveService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="service">Service</param>
        public DriveController(IMyApp app, IHttpContextAccessor httpContextAccessor, IDriveService service)
            : base(app, httpContextAccessor)
        {
            this.service = service;
        }

        /// <summary>
        /// Delete file
        /// 删除文件
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        [HttpDelete("Delete/{id}")]
        [Roles(UserRole.Founder | UserRole.Admin)]
        public async Task Delete(string id)
        {
            var result = await service.DeleteAsync(id, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Download file
        /// 下载文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Task</returns>
        [HttpGet("DownloadFile/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            var result = await service.DownloadFileAsync(id);
            if (result == null) return NoContent();
            return File(result.Value.data, result.Value.contentType, result.Value.fileName);
        }

        /// <summary>
        /// Query files
        /// 查询文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("Query")]
        public async Task Query(DriveQueryRQ rq)
        {
            await service.QueryAsync(rq, Response);
        }

        /// <summary>
        /// Share file
        /// 分享文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        [HttpPut("ShareFile")]
        public async Task ShareFile(DriveShareFileRQ rq)
        {
            var result = await service.ShareFileAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Upload files
        /// 上传文件
        /// </summary>
        /// <param name="file">File</param>
        /// <returns>Task</returns>
        [HttpPost("UploadFiles")]
        [Roles(UserRole.User | UserRole.Founder | UserRole.Admin)]
        [RequestSizeLimit(262144000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
        public async Task UploadFiles(IEnumerable<IFormFile> files)
        {
            if (files.Count() > 5 || files.Any(file => file.Length > 52428800))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult());
                return;
            }

            var result = await service.UploadFilesAsync(files, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Remove file share
        /// 移除文件分享
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        [HttpPut("RemoveShare/{id}")]
        public async Task RemoveShare(string id)
        {
            var result = await service.RemoveShareAsync(id);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Update file
        /// 更新文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("Update")]
        public async Task Update(DriveUpdateRQ rq)
        {
            var result = await service.UpdateAsync(rq, Ip);
            await WriteResultAsync(result);
        }
    }
}
