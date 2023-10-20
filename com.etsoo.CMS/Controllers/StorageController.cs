﻿using com.etsoo.CMS.Application;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.Services;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Storage controller
    /// 存储控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class StorageController : SharedController
    {
        readonly IMyApp app;
        readonly IStorage storage;
        readonly ILogger<StorageController> _logger;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Accessor</param>
        /// <param name="storage">Storage</param>
        /// <param name="logger">Logger</param>
        public StorageController(IMyApp app, IHttpContextAccessor httpContextAccessor, IStorage storage, ILogger<StorageController> logger)
            : base(app, httpContextAccessor)
        {
            this.app = app;
            this.storage = storage;
            _logger = logger;
        }

        /// <summary>
        /// Get online drive file
        /// 获取网络硬盘文件
        /// </summary>
        /// <param name="id">File id</param>
        /// <returns>Task</returns>
        [HttpGet("OnlineDrive/{id}")]
        public async Task OnlineDrive([StringLength(32, MinimumLength = 12)] string id, [FromQuery] string? key = null)
        {
            var repo = new DriveRepo(app, null);
            var file = await repo.ReadAsync(id);
            if (file == null)
            {
                await Response.WriteAsync("No file matched (没有找到匹配的文件)");
                return;
            }

            if (!file.Shared && !DriveService.ValidateAccessKey(app, id, key))
            {
                await Response.WriteAsync("Not passing the correct access key (没有传递正确的访问密匙)");
                return;
            }

            using var stream = await storage.ReadAsync(file.Path);
            if (stream == null)
            {
                await Response.WriteAsync("File has been deleted (文件已经删除)");
                return;
            }
            else
            {
                await stream.CopyToAsync(Response.Body);
            }
        }

        /// <summary>
        /// Get static resources
        /// 获取动态资源
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Task</returns>
        [HttpGet("Resources/{*path}")]
        public async Task Resources(string path)
        {
            using var stream = await storage.ReadAsync($"/Resources/{path}");
            if (stream != null)
                await stream.CopyToAsync(Response.Body);
        }
    }
}
