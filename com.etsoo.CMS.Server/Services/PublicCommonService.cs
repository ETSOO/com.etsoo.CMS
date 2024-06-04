using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Server.Defs;
using com.etsoo.Database;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Public common service
    /// 公共的通用服务
    /// </summary>
    public class PublicCommonService : CommonService, IPublicCommonService
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
        public PublicCommonService(IMyApp app, IMyUserAccessor userAccessor, ILogger<PublicCommonService> logger)
            : base(app, userAccessor.User, "public", logger)
        {
        }

        /// <summary>
        /// Read drive file
        /// 读取网络硬盘文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<DriveFile?> ReadDriveAsync(string id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 30));

            var command = CreateCommand($"SELECT id, name, path, contentType, shared FROM files WHERE id = @{nameof(id)}", parameters, cancellationToken: cancellationToken);

            return await QueryAsAsync<DriveFile>(command);
        }

        /// <summary>
        /// Read service (plugin)
        /// 读取服务（插件）
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<DbService> ReadServiceAsync(string id, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id);
            var command = CreateCommand($"SELECT app, secret FROM services WHERE id = @{nameof(id)} AND status < 200", parameters, cancellationToken: cancellationToken);
            var result = await QueryAsAsync<DbService>(command);
            if (result == null) return new DbService(id, string.Empty);
            return result with { Secret = App.DecriptData(result.Secret) };
        }
    }
}