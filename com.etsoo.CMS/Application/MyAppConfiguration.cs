using com.etsoo.ServiceApp.Application;
using com.etsoo.Utils.Crypto;

namespace com.etsoo.CMS.Application
{
    public record MyAppConfiguration : ServiceAppConfiguration
    {
        private string? _publicStaticToken;

        /// <summary>
        /// Static token for public access
        /// 用于公共访问的静态令牌
        /// Next.js integration example: https://vercel.com/guides/integrating-next-js-and-contentful-for-your-headless-cms
        /// </summary>
        public string? PublicStaticToken
        {
            get { return _publicStaticToken; }
            init { _publicStaticToken = value; }
        }

        public override void UnsealData(Func<string, string, string>? secureManager = null)
        {
            base.UnsealData(secureManager);

            if (!string.IsNullOrEmpty(PublicStaticToken))
            {
                _publicStaticToken = CryptographyUtils.UnsealData(nameof(PublicStaticToken), PublicStaticToken, secureManager);
            }
        }
    }
}
