using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Drive;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.HTTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using com.etsoo.Utils.String;
using System.Collections.Concurrent;
using System.Net;
using System.Web;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Online drive service
    /// 网络硬盘服务
    /// </summary>
    public class DriveService : CommonService<DriveRepo>, IDriveService
    {
        /// <summary>
        /// Encrypt access key
        /// 加密访问密匙
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="id">File id</param>
        /// <param name="hours">Validation hours</param>
        /// <returns>Result</returns>
        public static string EncryptAccessKey(IMyApp app, string id, int hours)
        {
            var utc = DateTime.UtcNow.AddHours(hours);
            var cipher = app.EncriptData($"{utc.ToBinary()}", $"sharefile-{id}");
            return cipher;
        }

        /// <summary>
        /// Validate access key
        /// 验证访问密匙
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="id">File id</param>
        /// <param name="key">Access key</param>
        /// <returns>Result</returns>
        public static bool ValidateAccessKey(IMyApp app, string id, string? key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            try
            {
                var binaryText = app.DecriptData(key, $"sharefile-{id}");
                if (!long.TryParse(binaryText, out var binary)) return false;

                if (DateTime.FromBinary(binary) < DateTime.UtcNow) return false;
                else return true;
            }
            catch
            {
                return false;
            }
        }

        readonly IStorage storage;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        public DriveService(IMyApp app, IServiceUserAccessor userAccessor, ILogger<ArticleService> logger, IStorage storage)
            : base(app, new DriveRepo(app, userAccessor.UserSafe), logger)
        {
            // Optional injection
            // IEnumerable<IStorage> storages
            // storage = storages.FirstOrDefault();
            this.storage = storage;
        }

        /// <summary>
        /// Delete file
        /// 删除文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <param name="ip">IP address</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(string id, IPAddress ip)
        {
            // Query the file
            var file = await Repo.ReadAsync(id);
            if (file == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Delete the file first
            await storage.DeleteAsync(file.Path);

            // Delete from database
            var result = await Repo.DeleteAsync(id);

            // Audit
            await Repo.AddAuditAsync(AuditKind.OnlineDrive, id, $"Delete file {file.Name}", ip, result, new { Id = id });

            return result;
        }

        /// <summary>
        /// Download file
        /// 下载文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Result</returns>
        public async ValueTask<(Stream data, string fileName, string contentType)?> DownloadFileAsync(string id)
        {
            var file = await Repo.ReadAsync(id);
            if (file == null) return null;

            var stream = await storage.ReadAsync(file.Path);
            if (stream == null) return null;

            return (stream, file.Name, file.ContentType);
        }

        /// <summary>
        /// Query files
        /// 查询文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(DriveQueryRQ rq, HttpResponse response)
        {
            await Repo.QueryAsync(rq, response);
        }

        /// <summary>
        /// Share file
        /// 分享文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> ShareFileAsync(DriveShareFileRQ rq, IPAddress ip)
        {
            var file = await Repo.ReadAsync(rq.Id);
            if (file == null) return ApplicationErrors.NoId.AsResult("");

            if (!file.Shared && !rq.Hours.HasValue)
            {
                return ApplicationErrors.NoValidData.AsResult("hours");
            }

            var result = ActionResult.Success;
            if (file.Shared)
            {
                result.Data.Add("Id", storage.GetUrl($"/OnlineDrive/{rq.Id}"));
            }
            else
            {
                var cipher = EncryptAccessKey(App, rq.Id, rq.Hours.GetValueOrDefault());
                result.Data.Add("Id", storage.GetUrl($"/OnlineDrive/{rq.Id}?key={HttpUtility.UrlEncode(cipher)}"));
            }

            await Repo.AddAuditAsync(AuditKind.OnlineDrive, rq.Id, $"Share file {file.Name}", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Upload files
        /// 上传文件
        /// </summary>
        /// <param name="files">Files</param>
        /// <param name="ip">IP</param>
        /// <returns>Result</returns>
        /// <exception cref="InvalidDataException"></exception>
        public async ValueTask<IActionResult> UploadFilesAsync(IEnumerable<IFormFile> files, IPAddress ip)
        {
            // File path
            var path = $"/Resources/OnlineDrive/{DateTime.UtcNow:yyyyMM}";

            // Upload result
            var results = new ConcurrentQueue<(string fileName, IActionResult result)>();

            await Parallel.ForEachAsync(files, CancellationToken, async (file, CancellationToken) =>
            {
                var fileName = file.FileName;
                var fileSize = file.Length;
                var contentType = file.ContentType;

                var extension = MimeTypeMap.TryGetExtension(contentType);
                if (string.IsNullOrEmpty(extension))
                {
                    throw new InvalidDataException($"No extension found for file {fileName}");
                }

                // File path
                var filePath = $"{path}/{Path.GetRandomFileName()}{extension}";

                // Request data
                var rq = new DriveCreateRQ
                {
                    Id = Path.GetRandomFileName(),
                    Name = fileName,
                    Path = filePath,
                    Size = fileSize,
                    ContentType = contentType
                };

                ActionResult result;
                try
                {
                    // Save the stream to file directly
                    var saveResult = await storage.WriteAsync(filePath, file.OpenReadStream(), WriteCase.CreateNew);

                    if (saveResult)
                    {
                        await Repo.CreateAsync(rq);

                        result = ActionResult.Success;
                    }
                    else
                    {
                        result = ApplicationErrors.AccessDenied.AsResult("storage");
                    }
                }
                catch (Exception ex)
                {
                    result = LogException(ex);

                    // Delete the file
                    await storage.DeleteAsync(filePath);
                }

                // Hide sensitive file path data
                rq.Path = StringUtils.HideData(rq.Path);

                await Repo.AddAuditAsync(AuditKind.OnlineDrive, rq.Id, $"Upload file {fileName}", ip, result, rq);

                results.Enqueue((fileName, result));
            });

            var result = results.Any(result => result.result.Ok) ? ActionResult.Success : new ActionResult();
            result.Data.Add("items", results);
            return result;
        }

        /// <summary>
        /// Remove file share
        /// 移除文件分享
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> RemoveShareAsync(string id)
        {
            return await Repo.RemoveShareAsync(id);
        }

        /// <summary>
        /// Update file
        /// 更新文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(DriveUpdateRQ rq, IPAddress ip)
        {
            var fields = rq.ChangedFields ?? Enumerable.Empty<string>();
            var removeShare = fields.Contains("removeShare", StringComparer.OrdinalIgnoreCase);

            IActionResult actionResult = new ActionResult();

            var fieldCount = removeShare ? 2 : 1;
            if (fields.Count() >= fieldCount)
            {
                var (result, _) = await Repo.InlineUpdateAsync(
                    rq,
                    new QuickUpdateConfigs(new[] { "name", "shared" })
                    {
                        TableName = "files",
                        IdField = "id"
                    }, null, null, Logger
                 );
                actionResult = result;
            }

            if (removeShare)
            {
                actionResult = await RemoveShareAsync(rq.Id);
            }

            await Repo.AddAuditAsync(AuditKind.OnlineDrive, rq.Id, $"Update file {rq.Id}", ip, actionResult, rq);

            return actionResult;
        }
    }
}