using System.Net;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Refresh token data
    /// 刷新令牌数据
    /// </summary>
    /// <param name="Device">Device</param>
    /// <param name="Ip">IP address</param>
    /// <param name="Pwd">Password</param>
    /// <param name="Timezone">Timezone</param>
    public record RefreshTokenDto(string Device, IPAddress Ip, string? Pwd, string? Timezone);
}
