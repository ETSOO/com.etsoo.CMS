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
    /// <param name="Device">Device</param>
    /// <param name="Region">Country or region</param>
    /// <param name="Timezone">Timezone</param>
    public record LoginDto(string Id, string Pwd, IPAddress Ip, string Device, string Region, string? Timezone);
}
