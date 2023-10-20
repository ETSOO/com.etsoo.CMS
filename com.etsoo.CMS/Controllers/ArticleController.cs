using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Web;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Website article controller
    /// 网站文章控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : SharedController
    {
        // Service
        readonly IArticleService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="service">Service</param>
        public ArticleController(IMyApp app, IHttpContextAccessor httpContextAccessor, IArticleService service)
            : base(app, httpContextAccessor)
        {
            this.service = service;
        }

        /// <summary>
        /// Create article
        /// 创建文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("Create")]
        public async Task Create(ArticleCreateRQ rq)
        {
            var result = await service.CreateAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Delete Article
        /// 删除文章
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
        /// Delete photo
        /// 删除照片
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("DeletePhoto")]
        [Roles(UserRole.Founder | UserRole.Admin)]
        public async Task DeletePhoto(ArticleDeletePhotoRQ rq)
        {
            var result = await service.DeletePhotoAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Query article
        /// 查询文章
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("Query")]
        public async Task<DbArticleQuery[]> Query(ArticleQueryRQ rq)
        {
            return await service.QueryAsync(rq);
        }

        /// <summary>
        /// Query history
        /// 查询操作历史
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        [HttpPost("QueryHistory/{id:int}")]
        public async Task QueryHistory(int id)
        {
            await service.QueryHistoryAsync(id, Response);
        }

        /// <summary>
        /// Sort gallery photos
        /// 图库照片排序
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("SortPhotos")]
        public async Task SortPhotos(ArticleSortPhotosRQ rq)
        {
            var result = await service.SortPhotosAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Translate
        /// 翻译
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPost("Translate")]
        public async Task<string> Translate(TranslateRQ rq)
        {
            return await service.TranslateAsync(rq.Text);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.User | UserRole.Founder | UserRole.Admin)]
        [HttpPut("Update")]
        public async Task Update(ArticleUpdateRQ rq)
        {
            var result = await service.UpdateAsync(rq, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Update photo gallery item
        /// 更新图片库项目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("UpdatePhoto")]
        [Roles(UserRole.User | UserRole.Founder | UserRole.Admin)]
        public async Task UpdatePhoto(ArticleUpdatePhotoRQ rq)
        {
            var result = await service.UpdatePhotoAsync(rq, Ip);
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
            var url = await service.UploadLogoAsync(id, stream, logo.ContentType, Ip) ?? throw new ApplicationException();
            return url;
        }

        /// <summary>
        /// Update photos
        /// 上传照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <param name="files">Photo files</param>
        /// <returns>Task</returns>
        [HttpPost("UploadPhotos/{id:int}")]
        [RequestSizeLimit(52428800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        public async Task UploadPhotos(int id, IEnumerable<IFormFile> files)
        {
            if (files.Count() > 5 || files.Any(file => file.Length > 10485760))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult());
                return;
            }

            var result = await service.UploadPhotosAsync(id, files, Ip);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        [HttpGet("UpdateRead/{id:int}")]
        public async Task UpdateRead(int id)
        {
            await service.UpdateReadAsync(id, Response);
        }

        /// <summary>
        /// Read gallery photos
        /// 阅读图库照片
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        [HttpGet("ViewGallery/{id:int}")]
        public async Task ViewGallery(int id)
        {
            await service.ViewGalleryAsync(id, Response);
        }

        /// <summary>
        /// Read for view
        /// 阅读浏览
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns>Task</returns>
        [HttpGet("ViewRead/{id:int}")]
        public async Task ViewRead(int id)
        {
            await service.ViewReadAsync(id, Response);
        }
    }
}
