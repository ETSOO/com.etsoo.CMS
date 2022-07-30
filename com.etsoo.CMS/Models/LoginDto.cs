using System.Net;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Login Data
    /// 登录数据
    /// </summary>
    /// <param name="Id">User id</param>
    /// <param name="Pwd">Password</param>
    /// <param name="Ip">IP address</param>
    public record LoginDto(string Id, string Pwd, IPAddress Ip);
}
