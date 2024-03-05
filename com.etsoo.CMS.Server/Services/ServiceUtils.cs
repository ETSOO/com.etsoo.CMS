using com.etsoo.CMS.Models;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// Format keywords
        /// 格式化关键词
        /// </summary>
        /// <param name="keywords">Keywords</param>
        /// <returns>Result</returns>
        public static string FormatKeywords(string keywords)
        {
            // Unify the format
            var items = new Regex(@"\s*[;；,，]+\s*", RegexOptions.Multiline).Split(keywords);
            return string.Join(", ", items);
        }

        /// <summary>
        /// Parse options
        /// 解析配置选项
        /// </summary>
        /// <typeparam name="T">Generic option type</typeparam>
        /// <param name="options">JSON options</param>
        /// <returns>Result</returns>
        public static T? ParseOptions<T>(string? options) where T : class
        {
            if (string.IsNullOrEmpty(options) || !options.StartsWith("{") || !options.EndsWith("}")) return default;
            return JsonSerializer.Deserialize<T>(options, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
