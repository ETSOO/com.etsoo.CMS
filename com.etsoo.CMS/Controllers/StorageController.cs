using com.etsoo.CMS.Application;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        readonly IStorage storage;
        ILogger<StorageController> _logger;

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
            this.storage = storage;
            _logger = logger;
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
