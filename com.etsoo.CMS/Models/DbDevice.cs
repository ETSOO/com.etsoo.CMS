using com.etsoo.SourceGenerators.Attributes;
using com.etsoo.Utils.String;

namespace com.etsoo.CMS.Models
{
    /// <summary>
    /// Device token data
    /// 设备令牌数据
    /// </summary>
    /// <param name="Token">Refresh token</param>
    /// <param name="Creation">Creation</param>
    [AutoDataReaderGenerator(UtcDateTime = true)]
    public partial record DbDevice(string Token, DateTime Creation)
    {
        public DbDevice(string token, string creation) : this(token, StringUtils.TryParse<DateTime>(creation).GetValueOrDefault())
        {
        }
    }
}
