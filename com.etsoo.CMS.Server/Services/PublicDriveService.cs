using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Server.Defs;
using com.etsoo.Database;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Public online drive service
    /// 公开的在线驱动服务
    /// </summary>
    public class PublicDriveService : CommonService, IPublicDriveService
    {
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

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        public PublicDriveService(IMyApp app, IMyUserAccessor userAccessor, ILogger<PublicDriveService> logger)
            : base(app, userAccessor.User, "drive", logger)
        {
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
    }
}