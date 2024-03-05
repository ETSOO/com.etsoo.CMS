using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Web;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Website tab controller
    /// 网站栏目控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TabController : SharedController
    {
        // Service
        readonly ITabService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="service">Service</param>
        public TabController(IMyApp app, IHttpContextAccessor httpContextAccessor, ITabService service)
            : base(app, httpContextAccessor)
        {
            this.service = service;
        }

        /// <summary>
        /// Create tab
        /// 创建栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("Create")]
        public async Task Create(TabCreateRQ rq)
        {
            var result = await service.CreateAsync(rq, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Delete tab
        /// 删除栏目
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        [HttpDelete("Delete/{id:int}")]
        [Roles(UserRole.Founder | UserRole.Admin)]
        public async Task Delete(int id)
        {
            var result = await service.DeleteAsync(id, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Tab list
        /// 栏目列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("List")]
        public async Task List(TiplistRQ rq)
        {
            await service.ListAsync(rq, Response, CancellationToken);
        }

        /// <summary>
        /// Query tab
        /// 查询栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns></returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPost("Query")]
        public async Task Query(TabQueryRQ rq)
        {
            await service.QueryAsync(rq, Response, CancellationToken);
        }

        /// <summary>
        /// Sort
        /// 排序
        /// </summary>
        /// <param name="data">Data to sort</param>
        /// <returns>Task</returns>
        [HttpPut("Sort")]
        [Roles(UserRole.Founder | UserRole.Admin)]
        public async Task<int> Sort(Dictionary<int, short> data)
        {
            return await service.SortAsync(data, CancellationToken);
        }

        /// <summary>
        /// Update tab
        /// 更新栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("Update")]
        public async Task Update(TabUpdateRQ rq)
        {
            var result = await service.UpdateAsync(rq, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Update logo
        /// 更新照片
        /// </summary>
        /// <param name="logo">Logo form</param>
        /// <returns>Task</returns>
        [HttpPut("UploadLogo/{id:int}")]
        [Roles(UserRole.User | UserRole.Founder | UserRole.Admin)]
        [RequestSizeLimit(10485760)]
        public async Task<string> UploadLogo([FromRoute] int id, IFormFile logo)
        {
            using var stream = logo.OpenReadStream();
            var url = await service.UploadLogoAsync(id, stream, logo.ContentType, CancellationToken) ?? throw new ApplicationException();
            return url;
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpGet("UpdateRead/{id:int}")]
        public async Task UpdateRead(int id)
        {
            await service.UpdateReadAsync(id, Response, CancellationToken);
        }

        /// <summary>
        /// Read for ancestors
        /// 上层栏目浏览
        /// </summary>
        /// <param name="id">Tab id</param>
        /// <returns>Task</returns>
        [HttpGet("AncestorRead/{id:int}")]
        public async Task AncestorRead(int id)
        {
            await service.AncestorReadAsync(id, Response, CancellationToken);
        }
    }
}
