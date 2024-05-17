using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.CMS.RQ.User
{
    /// <summary>
    /// User create request data
    /// 用户创建请求数据
    /// </summary>
    [SqlInsertCommand("users", NamingPolicy.CamelCase, Database = DatabaseName.SQLite, PrimaryKey = "id", IgnoreExists = true)]
    public partial record UserCreateRQ
    {
        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        [Property(Length = 128)]
        public required string Id { get; set; }

        /// <summary>
        /// Role
        /// 角色
        /// </summary>
        public required UserRole Role { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; } = EntityStatus.Normal;

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool? Enabled
        {
            set
            {
                if (value.HasValue)
                {
                    Status = value.Value ? EntityStatus.Normal : EntityStatus.Inactivated;
                }
            }
        }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation => DateTime.Now;

        /// <summary>
        /// Password
        /// 密码
        /// </summary>
        [Property(Length = 128, IsAnsi = true)]
        [StringLength(128, MinimumLength = 6)]
        public required string Password { get; set; }
    }
}
