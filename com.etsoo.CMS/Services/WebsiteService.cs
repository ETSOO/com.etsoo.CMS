using com.etsoo.ApiProxy.Configs;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.Repo;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Repositories;
using com.etsoo.CoreFramework.User;
using com.etsoo.DI;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using com.etsoo.Utils.String;
using System.Net;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website service
    /// 网站业务逻辑服务
    /// </summary>
    public class WebsiteService : CommonService<WebsiteRepo>, IWebsiteService
    {
        readonly IFireAndForgetService fireService;
        readonly IStorage storage;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        public WebsiteService(IMyApp app, IServiceUserAccessor userAccessor, ILogger<WebsiteService> logger, IStorage storage, IFireAndForgetService fireService)
            : base(app, new WebsiteRepo(app, userAccessor.UserSafe), logger)
        {
            this.storage = storage;
            this.fireService = fireService;
        }

        /// <summary>
        /// Create or update resource
        /// 创建或更新资源
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="ip">IP address</param>
        /// <returns>Action result</returns>
        public async Task<IActionResult> CreateOrUpdateResourceAsync(ResourceCreateRQ rq, IPAddress ip)
        {
            var result = await Repo.CreateOrUpdateResourceAsync(rq);

            await Repo.AddAuditAsync(AuditKind.CreateResource, rq.Id, $"Create or update resource {rq.Id}", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Create service
        /// 创建服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateServiceAsync(ServiceCreateRQ rq, IPAddress ip)
        {
            var secret = rq.Secret;
            if (!string.IsNullOrEmpty(secret))
            {
                rq.Secret = App.EncriptData(secret);
                secret = StringUtils.HideData(secret);
            }

            var result = await Repo.CreateServiceAsync(rq);

            rq.Secret = secret;
            await Repo.AddAuditAsync(AuditKind.CreateService, rq.Id, $"Create service {rq.Id}", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Dashboard data
        /// 仪表盘数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task DashboardAsync(HttpResponse response)
        {
            await Repo.DashboardAsync(response);
        }

        /// <summary>
        /// Initialize website
        /// 初始化网站
        /// </summary>
        /// <param name="rq">Reqeust data</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> InitializeAsync(InitializeRQ rq)
        {
            return await Repo.InitializeAsync(rq);
        }

        /// <summary>
        /// Async on demand revalidation
        /// 异步按需重新验证
        /// </summary>
        /// <param name="urls">URLs</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> OnDemandRevalidateAsync(params string[] urls)
        {
            // NextJs
            var nextJs = await Repo.ReadServiceAsync(NextJsOptions.Name);
            if (nextJs != null)
            {
                var nextJsAddress = nextJs.App.TrimEnd('/');
                var nextJsToken = App.DecriptData(nextJs.Secret);

                var fUrls = urls.Select(url =>
                {
                    url = url.Trim();
                    if (url.StartsWith('/')) return url;
                    else if (url.StartsWith(nextJsAddress)) return url[nextJsAddress.Length..];
                    else return string.Empty;
                }).Where(url => !string.IsNullOrEmpty(url)).ToArray();

                Logger.LogDebug("Static page generation to {address} for urls: {urls}", nextJsAddress, fUrls);

                // Fire and forget
                fireService.FireAsync<IHttpClientFactory>(async (factory, logger) =>
                {
                    try
                    {
                        var client = factory.CreateClient();
                        var nextJsApi = new NextJsProxy(client, logger, new NextJsOptions
                        {
                            BaseAddress = nextJsAddress,
                            Token = nextJsToken
                        });
                        var result = await nextJsApi.OnDemandRevalidateAsync(fUrls);
                        if (!result.Ok)
                        {
                            logger.LogError("NextJs On-demand revalidataion failed: {@result}", result);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Next.js On-demand Revalidation failed / 按需重新验证失败");
                    }
                });

                return ActionResult.Success;
            }
            else
            {
                Logger.LogWarning("Static page generation for urls: {urls} not defined", urls);
                return ApplicationErrors.InvalidAction.AsResult();
            }
        }

        /// <summary>
        /// Get mobile QRCode image Base64 string
        /// 获取移动端QRCode图片的Base64字符串
        /// </summary>
        /// <returns>Base64 string</returns>
        public async Task<string> QRCodeAsync(string url)
        {
            url = url.Replace("{id}", Repo.User?.Id);

            var options = new BarcodeOptions
            {
                Type = "QRCode",
                Content = url,
                ForegroundText = "#3f51b5",
                Width = 360,
                Height = 360
            };

            return await Task.Run(() => BarcodeUtils.Create(options));
        }

        /// <summary>
        /// Read JSON data
        /// 读取 JSON 数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task ReadJsonDataAsync(HttpResponse response)
        {
            await Repo.ReadJsonDataAsync(response);
        }

        /// <summary>
        /// Read service (plugin)
        /// 读取服务（插件）
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        public async Task<DbService> ReadServiceAsync(string id)
        {
            var result = await Repo.ReadServiceAsync(id);
            if (result == null) return new DbService(id, string.Empty);
            return result with { Secret = App.DecriptData(result.Secret) };
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        public async Task ReadSettingsAsync(HttpResponse response)
        {
            await Repo.ReadSettingsAsync(response, storage.GetUrl(string.Empty));
        }

        /// <summary>
        /// Regenerate all tab URLs
        /// 重新生成所有栏目网址
        /// </summary>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> RegenerateTabUrlsAsync()
        {
            var tabs = await Repo.QueryTabsAsync();
            var urls = tabs.Select(tab => tab.GetUrl()).Where(url => !url.Equals('#') && !url.Equals('/')).ToArray();
            return await OnDemandRevalidateAsync(urls);
        }


        /// <summary>
        /// Query resources
        /// 查询资源
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryResourcesAsync(HttpResponse response)
        {
            await Repo.QueryResourcesAsync(response);
        }

        /// <summary>
        /// Query plugin services
        /// 查询插件服务
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        public async Task QueryServicesAsync(HttpResponse response)
        {
            await Repo.QueryServicesAsync(response);
        }

        /// <summary>
        /// Update resource URL
        /// 更新资源路径
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> UpdateResurceUrlAsync(WebsiteUpdateResurceUrlRQ rq, IPAddress ip)
        {
            await Repo.UpdateResurceUrlAsync(rq.OldResourceUrl, storage.GetUrl(string.Empty));

            await Repo.AddAuditAsync(AuditKind.UpdateResurceUrl, "website", $"Update website resource URL", rq, ip);

            return ActionResult.Success;
        }

        /// <summary>
        /// Update settings
        /// 更新设置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateSettingsAsync(WebsiteUpdateSettingsRQ rq, IPAddress ip)
        {
            if (!string.IsNullOrEmpty(rq.Domain))
            {
                rq.Domain = rq.Domain.TrimEnd('/');
            }

            if (!string.IsNullOrEmpty(rq.Keywords))
            {
                rq.Keywords = ServiceUtils.FormatKeywords(rq.Keywords);
            }

            // View website
            var data = await Repo.ReadSettingsAsync();
            if (data == null)
            {
                var result = await Repo.CreateSettingsAsync(rq);
                await Repo.AddAuditAsync(AuditKind.UpdateWebsiteSettings, rq.Id.ToString(), $"Create Website Settings", ip, result, rq);
                return result;
            }
            else
            {
                var newRQ = rq with { Id = data.RowId };
                var (result, _) = await Repo.InlineUpdateAsync<int, UpdateModel<int>>(newRQ, new QuickUpdateConfigs(new[] { "Domain", "Title", "Description", "Keywords", "JsonData" })
                {
                    TableName = "website",
                    IdField = "rowid"
                });

                if (result.Ok)
                {
                    // Revalidation all tabs except home
                    var tabs = await Repo.QueryTabsAsync();
                    var urls = tabs.Select(tab => tab.GetUrl()).Where(item => item != "/" && !item.Equals("#"));
                    if (urls.Any())
                    {
                        await OnDemandRevalidateAsync(urls.ToArray());
                    }
                }

                // Audit Json
                var json = result.Ok && rq.ChangedFields != null ? await SharedUtils.JoinAsAuditJsonAsync(data, newRQ, rq.ChangedFields) : null;

                await Repo.AddAuditAsync(AuditKind.UpdateWebsiteSettings, newRQ.Id.ToString(), $"Update Website Settings", ip, result, json);

                return result;
            }
        }

        /// <summary>
        /// Update service
        /// 更新服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateServiceAsync(ServiceUpdateRQ rq, IPAddress ip)
        {
            var secret = rq.Secret;
            if (!string.IsNullOrEmpty(secret))
            {
                rq.Secret = App.EncriptData(secret);
                secret = StringUtils.HideData(secret);
            }

            var parameters = new Dictionary<string, object>
            {
                ["RefreshTime"] = DateTime.UtcNow.ToString("u")
            };
            var (result, _) = await Repo.InlineUpdateAsync(rq, new QuickUpdateConfigs(new[] { "app", "secret", "status AS enabled=IIF(@Enabled, 0, 200)" })
            {
                TableName = "services",
                IdField = "id"
            }, "refreshTime = @RefreshTime", parameters);

            rq.Secret = secret;
            await Repo.AddAuditAsync(AuditKind.UpdateService, rq.Id, $"Update service {rq.Id}", ip, result, rq);

            return result;
        }

        /// <summary>
        /// Upgrade system
        /// 升级系统
        /// </summary>
        /// <returns>Task</returns>
        public async Task<IActionResult> UpgradeSystemAsync()
        {
            return await Repo.UpgradeSystemAsync();
        }
    }
}