using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.RQ.User;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Web;
using Microsoft.AspNetCore.Mvc;

namespace com.etsoo.CMS.Controllers
{
    /// <summary>
    /// Logined user controller
    /// 已登录用户控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : SharedController
    {
        // Service
        readonly IUserService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="service">Service</param>
        public UserController(IMyApp app, IHttpContextAccessor httpContextAccessor, IUserService service)
            : base(app, httpContextAccessor)
        {
            this.service = service;
        }

        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Task</returns>
        [HttpPut("ChangePassword")]
        public async Task ChangePassword(ChangePasswordRQ model)
        {
            // Check device
            if (!CheckDevice(service, model.DeviceId, out var checkResult, out var cd))
            {
                await WriteResultAsync(checkResult);
                return;
            }
            var deviceCore = cd.Value.DeviceCore;

            var oldPassword = service.DecryptDeviceData(model.OldPassword, deviceCore);
            if (string.IsNullOrEmpty(oldPassword))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("OldPassword"));
                return;
            }

            var password = service.DecryptDeviceData(model.Password, deviceCore);
            if (string.IsNullOrEmpty(password))
            {
                await WriteResultAsync(ApplicationErrors.NoValidData.AsResult("Password"));
                return;
            }

            var dto = new ChangePasswordDto(oldPassword, password);

            // Action result
            var changeResult = await service.ChangePasswordAsync(dto, CancellationToken);

            // Output
            await WriteResultAsync(changeResult);
        }

        /// <summary>
        /// Create user
        /// 创建用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("Create")]
        public async Task Create(UserCreateRQ rq)
        {
            var result = await service.CreateAsync(rq, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Delete user
        /// 删除用户
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        [HttpDelete("Delete/{id}")]
        [Roles(UserRole.Founder | UserRole.Admin)]
        public async Task Delete(string id)
        {
            var result = await service.DeleteAsync(id, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Query history user
        /// 查询操作历史用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPost("History")]
        public async Task History(UserHistoryQueryRQ rq)
        {
            await service.HistoryAsync(rq, Response, CancellationToken);
        }

        /// <summary>
        /// Query user
        /// 查询用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns></returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPost("Query")]
        public async Task Query(UserQueryRQ rq)
        {
            await service.QueryAsync(rq, Response, CancellationToken);
        }

        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("ResetPassword")]
        public async Task ResetPassword(UserResetPasswordRQ rq)
        {
            // Check device
            if (!CheckDevice(service, rq.DeviceId, out var checkResult, out var cd))
            {
                await WriteResultAsync(checkResult);
                return;
            }
            var deviceCore = cd.Value.DeviceCore;

            var result = await service.ResetPasswordAsync(rq.Id, deviceCore, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Signout
        /// 退出
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [HttpPut("Signout")]
        public async Task<bool> Signout(SignoutRQ rq)
        {
            // Check device
            if (!CheckDevice(service, rq.DeviceId, out var checkResult, out var cd))
            {
                await WriteResultAsync(checkResult);
                return false;
            }
            var deviceCore = cd.Value.DeviceCore;

            await service.SignoutAsync(deviceCore, CancellationToken);
            return true;
        }

        /// <summary>
        /// Update user
        /// 更新用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpPut("Update")]
        public async Task Update(UserUpdateRQ rq)
        {
            var result = await service.UpdateAsync(rq, CancellationToken);
            await WriteResultAsync(result);
        }

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Task</returns>
        [Roles(UserRole.Founder | UserRole.Admin)]
        [HttpGet("UpdateRead/{id}")]
        public async Task UpdateRead(string id)
        {
            await service.UpdateReadAsync(id, Response, CancellationToken);
        }
    }
}
