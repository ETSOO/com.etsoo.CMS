using com.etsoo.ApiProxy.Configs;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.CMS.Server;
using com.etsoo.CMS.Server.Defs;
using com.etsoo.CMS.Server.Services;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Json;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Database;
using com.etsoo.DI;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using com.etsoo.Utils.String;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace com.etsoo.CMS.Services
{
    /// <summary>
    /// Website service
    /// 网站业务逻辑服务
    /// </summary>
    public class WebsiteService : CommonUserService, IWebsiteService
    {
        readonly IPAddress ip;
        readonly IFireAndForgetService fireService;
        readonly IStorage storage;
        readonly IPublicCommonService publicService;

        const string TabJsonData = "ETSOO_CMS_TAB_JSON_DATA";
        const string ArticleJsonData = "ETSOO_CMS_ARTICLE_JSON_DATA";

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        public WebsiteService(IMyApp app, IMyUserAccessor userAccessor, ILogger<WebsiteService> logger, IStorage storage, IFireAndForgetService fireService, IPublicCommonService publicService)
            : base(app, userAccessor.UserSafe, "website", logger)
        {
            ip = userAccessor.Ip;
            this.storage = storage;
            this.fireService = fireService;
            this.publicService = publicService;
        }

        /// <summary>
        /// Create or update resource
        /// 创建或更新资源
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action result</returns>
        public async Task<IActionResult> CreateOrUpdateResourceAsync(ResourceCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // JSON Schema validation
            if (rq.Id.Equals(TabJsonData, StringComparison.OrdinalIgnoreCase) || rq.Id.Equals(ArticleJsonData, StringComparison.OrdinalIgnoreCase))
            {
                if (!CustomFieldSchema.Create().Evaluate(JsonNode.Parse(rq.Value)).IsValid)
                {
                    return ApplicationErrors.SchemaValidationError.AsResult("CustomFieldSchema");
                }

                rq.Id = rq.Id.ToUpper();
            }

            var parameters = FormatParameters(rq);

            AddSystemParameters(parameters);

            var command = CreateCommand($@"INSERT OR REPLACE INTO resources (id, value) VALUES (@{nameof(rq.Id)}, @{nameof(rq.Value)})", parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);

            var result = ActionResult.Success;

            var auditTitle = Resources.CreateResource.Replace("{0}", rq.Id);
            await AddAuditAsync(AuditKind.CreateResource, rq.Id, auditTitle, ip, result, rq, MyJsonSerializerContext.Default.ResourceCreateRQ, cancellationToken);

            return result;
        }

        /// <summary>
        /// Create service
        /// 创建服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateServiceAsync(ServiceCreateRQ rq, CancellationToken cancellationToken = default)
        {
            var secret = rq.Secret;
            if (!string.IsNullOrEmpty(secret))
            {
                rq.Secret = App.EncriptData(secret);
                secret = StringUtils.HideData(secret);
            }

            var parameters = FormatParameters(rq);

            var command = CreateCommand($@"INSERT INTO services (id, app, secret, status, refreshTime)
                VALUES (@{nameof(rq.Id)}, @{nameof(rq.App)}, @{nameof(rq.Secret)}, IIF(@{nameof(rq.Enabled)}, 0, 200), DATETIME('now', 'utc'))", parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);

            var result = ActionResult.Success;

            rq.Secret = secret;
            var auditTitle = Resources.CreateService.Replace("{0}", rq.Id);
            await AddAuditAsync(AuditKind.CreateService, rq.Id, auditTitle, ip, result, rq, MyJsonSerializerContext.Default.ServiceCreateRQ, cancellationToken);

            return result;
        }

        /// <summary>
        /// Create settings
        /// 创建设置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async Task<IActionResult> CreateSettingsAsync(WebsiteUpdateSettingsRQ rq, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(rq.Domain))
            {
                return ApplicationErrors.NoValidData.AsResult("domain");
            }

            if (string.IsNullOrEmpty(rq.Title))
            {
                return ApplicationErrors.NoValidData.AsResult("title");
            }

            var parameters = FormatParameters(rq);
            var command = CreateCommand($"INSERT INTO website(domain, title, description, keywords) VALUES (@{nameof(rq.Domain)}, @{nameof(rq.Title)}, @{nameof(rq.Description)}, @{nameof(rq.Keywords)})", parameters, cancellationToken: cancellationToken);
            await ExecuteAsync(command);

            return ActionResult.Success;
        }

        /// <summary>
        /// Dashboard data
        /// 仪表盘数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task DashboardAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();

            AddSystemParameters(parameters);

            var site = $"domain, version".ToJsonCommand(true);
            var article = $"id, title, refreshTime, author".ToJsonCommand();
            var audits = $"rowid, title, creation, author".ToJsonCommand();
            var command = CreateCommand(@$"SELECT {site} FROM website;
                SELECT {article} FROM (SELECT * FROM articles ORDER BY refreshTime DESC LIMIT 6);
                SELECT {audits} FROM (SELECT rowid, * FROM audits WHERE author = {SysUserField} AND kind <> {(byte)AuditKind.TokenLogin} ORDER BY rowid DESC LIMIT 4)", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response, ["site", "articles", "audits"]);
        }

        /// <summary>
        /// Initialize website
        /// 初始化网站
        /// </summary>
        /// <param name="rq">Reqeust data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> InitializeAsync(InitializeRQ rq, CancellationToken cancellationToken = default)
        {
            var parameters = FormatParameters(rq);
            var command = CreateCommand($"INSERT INTO website (domain, title) SELECT @{nameof(rq.Domain)}, @{nameof(rq.Title)} WHERE NOT EXISTS (SELECT * FROM website)", parameters, cancellationToken: cancellationToken);
            await ExecuteAsync(command);
            return ActionResult.Success;
        }

        /// <summary>
        /// Async on demand revalidation
        /// 异步按需重新验证
        /// </summary>
        /// <param name="urls">URLs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> OnDemandRevalidateAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default)
        {
            // NextJs
            var nextJs = await ReadServiceAsync(NextJsOptions.Name, cancellationToken);
            if (nextJs != null)
            {
                var nextJsAddress = nextJs.App.TrimEnd('/');
                var nextJsToken = nextJs.Secret;

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
        /// <param name="url">Url to generate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Base64 string</returns>
        public async Task<string> QRCodeAsync(string url, CancellationToken cancellationToken = default)
        {
            url = url.Replace("{id}", User?.Id);

            var options = new BarcodeOptions
            {
                Type = "QRCode",
                Content = url,
                ForegroundText = "#3f51b5",
                Width = 360,
                Height = 360
            };

            return await Task.Run(() => BarcodeUtils.Create(options), cancellationToken);
        }

        /// <summary>
        /// Read JSON data
        /// 读取 JSON 数据
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task ReadJsonDataAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            var json = $"json(jsonData) AS jsonData".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM website LIMIT 1", cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Read JSON data to object
        /// 读取 JSON 数据到对象
        /// </summary>
        /// <typeparam name="T">Generic object type</typeparam>
        /// <param name="typeInfo">Json type info</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<T?> ReadJsonDataAsync<T>(JsonTypeInfo<T>? typeInfo, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand($"SELECT jsonData FROM website LIMIT 1", cancellationToken: cancellationToken);
            var raw = await ExecuteScalarAsync<string?>(command);
            if (string.IsNullOrEmpty(raw)) return default;
            else if (typeInfo == null) return JsonSerializer.Deserialize<T>(raw, SharedUtils.JsonDefaultSerializerOptions);
            else return JsonSerializer.Deserialize(raw, typeInfo);
        }

        /// <summary>
        /// Read service (plugin)
        /// 读取服务（插件）
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<DbService> ReadServiceAsync(string id, CancellationToken cancellationToken = default)
        {
            return publicService.ReadServiceAsync(id, cancellationToken);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task ReadSettingsAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            await ReadSettingsAsync(response, storage.GetUrl(string.Empty), cancellationToken);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="response">HTTP Response</param>
        /// <param name="rootUrl">Root URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task ReadSettingsAsync(HttpResponse response, string? rootUrl, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(rootUrl), rootUrl);

            var json = $"domain, title, keywords, description, @{nameof(rootUrl)} AS rootUrl, json(jsonData) AS jsonData".ToJsonCommand(true);
            var command = CreateCommand($"SELECT {json} FROM website LIMIT 1", parameters, cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Read settings
        /// 读取设置
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async Task<DbWebsite?> ReadSettingsAsync(CancellationToken cancellationToken = default)
        {
            var command = CreateCommand($"SELECT rowid, domain, title, description, keywords, jsonData FROM website LIMIT 1", cancellationToken: cancellationToken);
            return await QueryAsAsync<DbWebsite>(command);
        }

        /// <summary>
        /// Regenerate all tab URLs
        /// 重新生成所有栏目网址
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> RegenerateTabUrlsAsync(CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(@$"SELECT id, name, layout, url FROM tabs WHERE status < 200", cancellationToken: cancellationToken);
            var tabs = await QueryAsListAsync<TabLink>(command);
            var urls = tabs.Select(tab => tab.GetUrl()).Where(url => !url.Equals('#') && !url.Equals('/')).ToArray();
            return await OnDemandRevalidateAsync(urls, cancellationToken);
        }


        /// <summary>
        /// Query resources
        /// 查询资源
        /// </summary>
        /// <param name="response">Response</param
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryResourcesAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            var json = $"id, value".ToJsonCommand();
            var command = CreateCommand($"SELECT {json} FROM resources", cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Query resource by id
        /// 通过ID查询资源
        /// </summary>
        /// <param name="Id">Resource id</param>
        /// <param name="response">Response</param
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryResourceAsync(string Id, HttpResponse response, CancellationToken cancellationToken = default)
        {
            var parameters = new DbParameters();
            parameters.Add(nameof(Id), Id.ToDbString());

            var command = CreateCommand($"SELECT value FROM resources WHERE id = @{nameof(Id)}", parameters, cancellationToken: cancellationToken);

            var value = await ExecuteScalarAsync<string>(command);

            await response.WriteAsync(value ?? "", cancellationToken);
        }

        /// <summary>
        /// Query article JSON data schema
        /// 查询文章JSON数据模式
        /// </summary>
        /// <param name="response">Response</param
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task QueryArticleJsonDataSchemaAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            await QueryResourceAsync(ArticleJsonData, response, cancellationToken);
        }

        /// <summary>
        /// Query tab JSON data schema
        /// 查询栏目JSON数据模式
        /// </summary>
        /// <param name="response">Response</param
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task QueryTabJsonDataSchemaAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            await QueryResourceAsync(TabJsonData, response, cancellationToken);
        }

        /// <summary>
        /// Query available tabs
        /// 查询可用栏目
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        private async Task<TabLink[]> QueryTabsAsync(CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(@$"SELECT id, name, layout, url FROM tabs WHERE status < 200", cancellationToken: cancellationToken);
            return await QueryAsListAsync<TabLink>(command);
        }

        /// <summary>
        /// Query plugin services
        /// 查询插件服务
        /// </summary>
        /// <param name="response">Response</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryServicesAsync(HttpResponse response, CancellationToken cancellationToken = default)
        {
            var json = $"id, app, refreshTime, {"status < 200".ToJsonBool()} AS enabled".ToJsonCommand();
            var command = CreateCommand($"SELECT {json} FROM services", cancellationToken: cancellationToken);

            await ReadJsonToStreamAsync(command, response);
        }

        /// <summary>
        /// Update resource URL
        /// 更新资源路径
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> UpdateResourceUrlAsync(WebsiteUpdateResurceUrlRQ rq, CancellationToken cancellationToken = default)
        {
            var oldField = nameof(rq.OldResourceUrl);
            var newField = nameof(rq.ResourceUrl);

            var parameters = new DbParameters();
            parameters.Add(oldField, rq.OldResourceUrl);
            parameters.Add(newField, rq.ResourceUrl);

            var command = CreateCommand($"""
                UPDATE tabs SET logo = REPLACE(logo, @{oldField}, @{newField});
                UPDATE articles SET logo = REPLACE(logo, @{oldField}, @{newField}), content = REPLACE(content, @{oldField}, @{newField}), slideshow = REPLACE(slideshow, @{oldField}, @{newField});
                """, parameters, cancellationToken: cancellationToken);

            await ExecuteAsync(command);

            var auditTitle = Resources.UpdateResourceUrl;
            await AddAuditAsync(AuditKind.UpdateResourceUrl, "website", auditTitle, rq, MyJsonSerializerContext.Default.WebsiteUpdateResurceUrlRQ, ip, cancellationToken: cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Update settings
        /// 更新设置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateSettingsAsync(WebsiteUpdateSettingsRQ rq, CancellationToken cancellationToken = default)
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
            var data = await ReadSettingsAsync(cancellationToken);
            if (data == null)
            {
                var result = await CreateSettingsAsync(rq, cancellationToken);
                await AddAuditAsync(AuditKind.UpdateWebsiteSettings, rq.Id.ToString(), Resources.CreateWebsiteSettings, ip, result, rq, cancellationToken: cancellationToken);
                return result;
            }
            else
            {
                var newRQ = rq with { Id = data.RowId };
                var (result, _) = await InlineUpdateAsync<int, UpdateModel<int>>(newRQ, new QuickUpdateConfigs(["Domain", "Title", "Description", "Keywords", "JsonData"])
                {
                    TableName = "website",
                    IdField = "rowid"
                }, cancellationToken: cancellationToken);

                if (result.Ok)
                {
                    // Revalidation all tabs except home
                    var tabs = await QueryTabsAsync(cancellationToken);
                    var urls = tabs.Select(tab => tab.GetUrl()).Where(item => item != "/" && !item.Equals("#"));
                    if (urls.Any())
                    {
                        await OnDemandRevalidateAsync(urls.ToArray(), cancellationToken);
                    }
                }

                // Audit Json
                var json = result.Ok && rq.ChangedFields != null ? await SharedUtils.JoinAsAuditJsonAsync(data, newRQ, rq.ChangedFields) : null;

                await AddAuditAsync(AuditKind.UpdateWebsiteSettings, newRQ.Id.ToString(), Resources.UpdateWebsiteSettings, ip, result, json, cancellationToken: cancellationToken);

                return result;
            }
        }

        /// <summary>
        /// Update service
        /// 更新服务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateServiceAsync(ServiceUpdateRQ rq, CancellationToken cancellationToken = default)
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
            var (result, _) = await InlineUpdateAsync(rq, new QuickUpdateConfigs(["app", "secret", "status AS enabled=IIF(@Enabled, 0, 200)"])
            {
                TableName = "services",
                IdField = "id"
            }, "refreshTime = @RefreshTime", parameters, cancellationToken);

            rq.Secret = secret;
            var auditTitle = Resources.UpdateService.Replace("{0}", rq.Id);
            await AddAuditAsync(AuditKind.UpdateService, rq.Id, auditTitle, ip, result, rq, MyJsonSerializerContext.Default.ServiceUpdateRQ, cancellationToken);

            return result;
        }

        /// <summary>
        /// Upgrade system
        /// 升级系统
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> UpgradeSystemAsync(CancellationToken cancellationToken = default)
        {
            // New version
            var newVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            if (newVersion == null)
            {
                return ApplicationErrors.NoValidData.AsResult("version");
            }

            // Current version
            // When null means initialization
            var versionText = await ExecuteScalarAsync<string>(CreateCommand("SELECT version FROM website", cancellationToken: cancellationToken));
            if (!string.IsNullOrEmpty(versionText))
            {
                var version = new Version(versionText);
                var result = version.CompareTo(newVersion);

                // Same versions
                if (result >= 0)
                {
                    return ActionResult.Success;
                }

                // Actions
                if (version.CompareTo(new Version("1.0.2")) < 0)
                {
                    var command102 = CreateCommand($@"
                        ALTER TABLE website ADD COLUMN jsonData TEXT;

                        ALTER TABLE services ADD COLUMN jsonData TEXT;

                        ALTER TABLE tabs ADD COLUMN logo TEXT;
                        ALTER TABLE tabs ADD COLUMN description TEXT;
                        ALTER TABLE tabs ADD COLUMN jsonData TEXT;

                        ALTER TABLE articles ADD COLUMN jsonData TEXT;
                    ", cancellationToken: cancellationToken);

                    await ExecuteAsync(command102);
                }

                if (version.CompareTo(new Version("1.0.3")) < 0)
                {
                    var command103 = CreateCommand($@"
                        ALTER TABLE tabs ADD COLUMN icon TEXT;
                    ", cancellationToken: cancellationToken);

                    await ExecuteAsync(command103);
                }

                if (version.CompareTo(new Version("1.0.4")) < 0)
                {
                    var command104 = CreateCommand($@"
                        CREATE TABLE IF NOT EXISTS files (
                            id TEXT PRIMARY KEY,
                            name TEXT NOT NULL,
                            path TEXT NOT NULL,
                            size INTEGER NOT NULL,
                            contentType TEXT NOT NULL,
                            shared INTEGER DEFAULT 0,
                            author TEXT NOT NULL,
                            creation TEXT NOT NULL
                        ) WITHOUT ROWID;

                        CREATE INDEX IF NOT EXISTS index_files_name ON files (name);
                        CREATE INDEX IF NOT EXISTS index_files_author ON files (author);
                        CREATE INDEX IF NOT EXISTS index_files_creation ON files (creation);
                    ", cancellationToken: cancellationToken);

                    await ExecuteAsync(command104);
                }
            }

            // Update version
            var updateParameters = new DbParameters();
            updateParameters.Add(nameof(newVersion), newVersion.ToString(3));
            var command = CreateCommand($"UPDATE website SET version = @{nameof(newVersion)}", updateParameters, cancellationToken: cancellationToken);
            await ExecuteAsync(command);

            return ActionResult.Success;
        }
    }
}