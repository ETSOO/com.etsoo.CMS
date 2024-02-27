using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.Utils.Actions;
using System.Text.Json.Serialization.Metadata;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Website service interface
    /// 网站业务逻辑服务接口
    /// </summary>
    public interface IWebsiteService : ICommonService
    {
        Task<IActionResult> CreateOrUpdateResourceAsync(ResourceCreateRQ rq, CancellationToken cancellationToken = default);

        Task<IActionResult> CreateServiceAsync(ServiceCreateRQ rq, CancellationToken cancellationToken = default);

        Task DashboardAsync(HttpResponse response, CancellationToken cancellationToken = default);

        Task<IActionResult> InitializeAsync(InitializeRQ rq, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> OnDemandRevalidateAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default);

        Task<string> QRCodeAsync(string url, CancellationToken cancellationToken = default);

        Task ReadJsonDataAsync(HttpResponse response, CancellationToken cancellationToken = default);

        Task<T?> ReadJsonDataAsync<T>(JsonTypeInfo<T>? typeInfo, CancellationToken cancellationToken = default);

        Task<DbService> ReadServiceAsync(string id, CancellationToken cancellationToken = default);

        Task ReadSettingsAsync(HttpResponse response, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> RegenerateTabUrlsAsync(CancellationToken cancellationToken = default);

        Task QueryResourcesAsync(HttpResponse response, CancellationToken cancellationToken = default);

        Task QueryServicesAsync(HttpResponse response, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdateResurceUrlAsync(WebsiteUpdateResurceUrlRQ rq, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdateSettingsAsync(WebsiteUpdateSettingsRQ rq, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdateServiceAsync(ServiceUpdateRQ rq, CancellationToken cancellationToken = default);

        Task<IActionResult> UpgradeSystemAsync(CancellationToken cancellationToken = default);
    }
}
