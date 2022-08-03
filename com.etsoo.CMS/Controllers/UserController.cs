using com.etsoo.CMS.Application;
using com.etsoo.CMS.Services;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
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
        readonly UserService service;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="logger">Logger</param>
        public UserController(IMyApp app, IHttpContextAccessor httpContextAccessor, ILogger<UserController> logger)
            : base(app, httpContextAccessor)
        {
            service = new UserService(app, ServiceUser.CreateSafe(httpContextAccessor.HttpContext), logger);
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
            var changeResult = await service.ChangePasswordAsync(dto, cd.Value.Ip);

            // Output
            await WriteResultAsync(changeResult);
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

            await service.SignoutAsync(deviceCore);
            return true;
        }
    }
}
