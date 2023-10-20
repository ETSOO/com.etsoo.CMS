using com.etsoo.CMS.Application;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Drive;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Repo
{
    /// <summary>
    /// Online drive repository
    /// 网络硬盘仓库
    /// </summary>
    public class DriveRepo : CommonRepo
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="user">User</param>
        public DriveRepo(IMyApp app, IServiceUser? user)
            : base(app, "drive", user)
        {

        }

        /// <summary>
        /// Create file
        /// 创建文件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Action result</returns>
        public async Task<int> CreateAsync(DriveCreateRQ rq)
        {
            var parameters = FormatParameters(rq);

            // versus DATETIME('now', 'utc')
            // var now = DateTime.UtcNow.ToString("u");
            // parameters.Add(nameof(now), now);

            AddSystemParameters(parameters);

            var sql = @$"INSERT INTO files (id, name, path, size, contentType, author, creation)
                VALUES (@{nameof(rq.Id)}, @{nameof(rq.Name)}, @{nameof(rq.Path)}, @{nameof(rq.Size)}, @{nameof(rq.ContentType)}, {SysUserField}, DATETIME('now', 'utc'))";

            var command = CreateCommand(sql, parameters);

            return await ExecuteAsync(command);
        }

        /// <summary>
        /// Delete file
        /// 删除文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> DeleteAsync(string id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 30));

            AddSystemParameters(parameters);

            var command = CreateCommand($"DELETE FROM files WHERE id = @{nameof(id)}", parameters);

            var result = await ExecuteAsync(command);

            if (result > 0)
                return ActionResult.Success;
            else
                return ApplicationErrors.NoId.AsResult();
        }

        /// <summary>
        /// Read file
        /// 读取文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Result</returns>
        public async Task<DriveFile?> ReadAsync(string id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 30));

            var command = CreateCommand($"SELECT id, name, path, contentType, shared FROM files WHERE id = @{nameof(id)}", parameters);

            return await QueryAsAsync<DriveFile>(command);
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
            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

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
            var command = CreateCommand(sql, parameters);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Remove file share
        /// 移除文件分享
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Action result</returns>
        public async ValueTask<IActionResult> RemoveShareAsync(string id)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(id), id.ToDbString(true, 30));
            parameters.Add("newid", Path.GetRandomFileName());

            AddSystemParameters(parameters);

            var command = CreateCommand($"UPDATE files SET id = @newid, shared = 0 WHERE id = @{nameof(id)}", parameters);

            var result = await ExecuteAsync(command);

            if (result > 0)
                return ActionResult.Success;
            else
                return ApplicationErrors.NoId.AsResult();
        }
    }
}
