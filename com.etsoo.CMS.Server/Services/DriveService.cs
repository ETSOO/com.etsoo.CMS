using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Drive;
using com.etsoo.CMS.Server.Services;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Database;
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
    public class DriveService : CommonUserService, IDriveService
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

        readonly IPAddress ip;
        readonly IStorage storage;
        private static readonly string[] updatableFields = ["name", "shared"];

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        public DriveService(IMyApp app, IMyUserAccessor userAccessor, ILogger<DriveService> logger, IStorage storage)
            : base(app, userAccessor.UserSafe, "drive", logger)
        {
            ip = userAccessor.Ip;

            // Optional injection
            // IEnumerable<IStorage> storages
            // storage = storages.FirstOrDefault();
            this.storage = storage;
        }

        /// <summary>
        /// Create file
        /// 创建文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action result</returns>
        private async Task<ActionResult> CreateAsync(DriveCreateRQ rq, CancellationToken cancellationToken = default)
        {
            /*
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var sql = @$"INSERT INTO files (id, name, path, size, contentType, author, creation)
                VALUES (@{nameof(rq.Id)}, @{nameof(rq.Name)}, @{nameof(rq.Path)}, @{nameof(rq.Size)}, @{nameof(rq.ContentType)}, {SysUserField}, DATETIME('now', 'utc'))";

            var command = CreateCommand(sql, parameters, cancellationToken: cancellationToken);

            return await ExecuteAsync(command);
            */

            var id = await SqlInsertAsync<DriveCreateRQ, string>(rq, cancellationToken);
            var result = id == null ? new ActionResult() { } : ActionResult.Success;

            return result;
        }

        /// <summary>
        /// Delete file
        /// 删除文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            // Query the file
            var file = await ReadAsync(id, cancellationToken);
            if (file == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Delete the file first
            await storage.DeleteAsync(file.Path);

            // Delete from database
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 30));

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM files WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            var recordsAffected = await ExecuteAsync(command);

            var result = recordsAffected > 0 ? ActionResult.Success : ApplicationErrors.NoId.AsResult();

            // Audit
            await AddAuditAsync(AuditKind.OnlineDrive, id, $"Delete file {file.Name}", ip, result, id, null, cancellationToken);

            return result;
        }

        /// <summary>
        /// Download file
        /// 下载文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(Stream data, string fileName, string contentType)?> DownloadFileAsync(string id, CancellationToken cancellationToken = default)
        {
            var file = await ReadAsync(id, cancellationToken);
            if (file == null) return null;

            var stream = await storage.ReadAsync(file.Path);
            if (stream == null) return null;

            return (stream, file.Name, file.ContentType);
        }

        /// <summary>
        /// Read file
        /// 读取文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<DriveFile?> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 30));

            var command = CreateCommand($"SELECT id, name, path, contentType, shared FROM files WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            return await QueryAsAsync<DriveFile>(command);
        }

        /// <summary>
        /// Query files
        /// 查询文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(DriveQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = FormatParameters(rq);

            var fields = $"id, name, size, author, shared, creation";
            var json = $"id, name, size, author, {"shared = 1".ToJsonBool()} AS shared, creation".ToJsonCommand();

            var items = new List<string>();
            if (!string.IsNullOrEmpty(rq.Name)) items.Add($"name LIKE '%' || @{nameof(rq.Name)} || '%'");
            if (!string.IsNullOrEmpty(rq.Author)) items.Add($"author = @{nameof(rq.Author)}");
            if (rq.CreationStart.HasValue) items.Add($"creation >= @{nameof(rq.CreationStart)}");
            if (rq.CreationEnd.HasValue) items.Add($"creation < @{nameof(rq.CreationEnd)} + 1");
            if (rq.Shared is true) items.Add($"shared = 1");
            else if (rq.Shared is false) items.Add($"shared = 0");

            var conditions = App.DB.JoinConditions(items);

            var limit = App.DB.QueryLimit(rq.BatchSize, rq.CurrentPage);

            // Sub-select, otherwise 'order by' fails
            var sql = $"SELECT {json} FROM (SELECT {fields} FROM files {conditions} {rq.GetOrderCommand()} {limit})";
            var command = CreateCommand(sql, parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Share file
        /// 分享文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> ShareFileAsync(DriveShareFileRQ rq, CancellationToken cancellationToken = default)
        {
            var file = await ReadAsync(rq.Id, cancellationToken);
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

            await AddAuditAsync(AuditKind.OnlineDrive, rq.Id, $"Share file {file.Name}", ip, result, rq, MyJsonSerializerContext.Default.DriveShareFileRQ, cancellationToken);

            return result;
        }

        /// <summary>
        /// Upload files
        /// 上传文件
        /// </summary>
        /// <param name="files">Files</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        /// <exception cref="InvalidDataException"></exception>
        public async ValueTask<IActionResult> UploadFilesAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken = default)
        {
            // File path
            var path = $"/Resources/OnlineDrive/{DateTime.UtcNow:yyyyMM}";

            // Upload result
            var results = new ConcurrentQueue<(string fileName, IActionResult result)>();

            await Parallel.ForEachAsync(files, cancellationToken, async (file, CancellationToken) =>
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
                    var saveResult = await storage.WriteAsync(filePath, file.OpenReadStream(), WriteCase.CreateNew, CancellationToken);

                    if (saveResult)
                    {
                        result = await CreateAsync(rq, cancellationToken);
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

                await AddAuditAsync(AuditKind.OnlineDrive, rq.Id, $"Upload file {fileName}", ip, result, rq, MyJsonSerializerContext.Default.DriveCreateRQ, CancellationToken);

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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> RemoveShareAsync(string id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 30));
            parameters.Add("newid", Path.GetRandomFileName());

            AddSystemParameters(parameters);

            var command = CreateCommand($"UPDATE files SET id = @newid, shared = 0 WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            var result = await ExecuteAsync(command);

            if (result > 0)
                return ActionResult.Success;
            else
                return ApplicationErrors.NoId.AsResult();
        }

        /// <summary>
        /// Update file
        /// 更新文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(DriveUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var fields = rq.ChangedFields ?? [];
            var removeShare = fields.Contains("removeShare", StringComparer.OrdinalIgnoreCase);

            IActionResult actionResult = new ActionResult();

            var fieldCount = removeShare ? 2 : 1;
            if (fields.Count() >= fieldCount)
            {
                var (result, _) = await InlineUpdateAsync(
                    rq,
                    new QuickUpdateConfigs(updatableFields)
                    {
                        TableName = "files",
                        IdField = "id"
                    }, null, null, cancellationToken
                 );
                actionResult = result;
            }

            if (removeShare)
            {
                actionResult = await RemoveShareAsync(rq.Id, cancellationToken);
            }

            await AddAuditAsync(AuditKind.OnlineDrive, rq.Id, $"Update file {rq.Id}", ip, actionResult, rq, MyJsonSerializerContext.Default.DriveUpdateRQ, cancellationToken);

            return actionResult;
        }
    }
}