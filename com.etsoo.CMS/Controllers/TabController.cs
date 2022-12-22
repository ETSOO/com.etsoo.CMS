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
        /// <param name="logger">Logger</param>
        /// <param name="service">Service</param>
        public TabController(IMyApp app, IHttpContextAccessor httpContextAccessor, ILogger<WebsiteController> logger, ITabService service)
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
            var result = await service.CreateAsync(rq, Ip);
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
            var result = await service.DeleteAsync(id);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Tab list
        /// 栏目列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("List")]
        public async Task List(TiplistRQ<int> rq)
        {
            await service.ListAsync(rq, Response);
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
            await service.QueryAsync(rq, Response);
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
            return await service.SortAsync(data);
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
            var result = await service.UpdateAsync(rq, Ip);
            await WriteResultAsync(result);
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
            await service.UpdateReadAsync(id, Response);
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
            await service.AncestorReadAsync(id, Response);
        }
    }
}
