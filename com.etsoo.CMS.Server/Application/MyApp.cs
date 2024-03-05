using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.Application;
using Microsoft.Data.Sqlite;

namespace com.etsoo.CMS.Application
{
    /// <summary>
    /// My app
    /// 我的程序
    /// </summary>
    public class MyApp : ServiceCommonApp<MyAppConfiguration, SqliteConnection>, IMyApp
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="services">Services dependency injection</param>
        /// <param name="configurationSection">Configuration section</param>
        /// <param name="modelValidated">Model DataAnnotations are validated or not</param>
        public MyApp(IServiceCollection services, IConfigurationSection configurationSection, bool modelValidated = false)
            : base(SetupApp(configurationSection, null, (data, connectionString) => (data.Get<MyAppConfiguration>(), new SqliteDatabase(connectionString))),
                  SetupAuth(services, configurationSection, null, null, (data) => data.Get<JwtSettings>()),
                  modelValidated)
        {
        }

        /// <summary>
        /// Override add system parameters
        /// 重写添加系统参数
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="parameters">Parameers</param>
        public override void AddSystemParameters(IServiceUser user, IDbParameters parameters)
        {
            parameters.Add(Constants.CurrentUserField, user.Id);
        }

        /// <summary>
        /// Build command name, ["member", "view"] => e2p_member_view
        /// 构建命令名称
        /// </summary>
        /// <param name="identifier">Identifier, like procedure with 'p'</param>
        /// <param name="parts">Parts</param>
        /// <param name="isSystem">Is system command</param>
        /// <returns>Result</returns>
        public override string BuildCommandName(string identifier, IEnumerable<string> parts, bool isSystem = false)
        {
            if (isSystem) return base.BuildCommandName(identifier, parts, isSystem);

            var command = $"e6{identifier}_" + string.Join("_", parts);
            return command.ToLower();
        }
    }
}
