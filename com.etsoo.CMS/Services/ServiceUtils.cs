using com.etsoo.CMS.Models;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using System.Diagnostics.CodeAnalysis;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Service utilities
    /// 服务工具
    /// </summary>
    public static class ServiceUtils
    {
        /// <summary>
        /// Check user
        /// 检查用户
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Result</returns>
        public static bool CheckUser([NotNullWhen(true)] DbUser? user, [NotNullWhen(false)] out IActionResult? result)
        {
            if (user == null)
            {
                // User not found error
                result = ApplicationErrors.NoUserFound.AsResult();
                return false;
            }

            // Frozen time check first
            if (user.FrozenTime != null && DateTime.UtcNow <= user.FrozenTime)
            {
                result = ApplicationErrors.UserFrozen.AsResult();
                result.Data.Add("FrozenTime", user.FrozenTime);
                return false;
            }

            // Status check
            if (user.Status >= EntityStatus.Inactivated)
            {
                result = ApplicationErrors.AccountDisabled.AsResult();
                return false;
            }

            result = null;
            return true;
        }
    }
}
