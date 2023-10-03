using AspNetCoreRateLimit;
using com.etsoo.ApiProxy.Configs;
using com.etsoo.ApiProxy.Defs;
using com.etsoo.ApiProxy.Proxy;
using com.etsoo.CMS.Application;
using com.etsoo.CMS.Defs;
using com.etsoo.CMS.Services;
using com.etsoo.CoreFramework.User;
using com.etsoo.DI;
using com.etsoo.Logs.Policies;
using com.etsoo.ServiceApp.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Custom settings
var configuration = builder.Configuration;
var RequestLogging = configuration.GetValue("RequestLogging", false);
var Cors = configuration.GetSection("Cors").Get<IEnumerable<string>?>()?.ToArray();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Destructure.With(new PIIAttributeMaskingDestructuringPolicy())
    .CreateLogger();

// Add services
var services = builder.Services;

// IP rate limit
services.AddOptions();
services.AddMemoryCache();
services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
services.AddInMemoryRateLimiting();
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Fire and forget
services.AddSingleton<IFireAndForgetService, FireAndForgetService>();

// Service app
var serviceApp = new MyApp(services, configuration.GetSection("EtsooWebsite"), true);

// Localization cultures
var Cultures = serviceApp.Configuration.Cultures;
if (Cultures == null || Cultures.Length == 0)
{
    Log.Logger.Error("No Supported Cultures Defined");
    return;
}

// Dependency Injection for the main application
// https://stackoverflow.com/questions/38138100/addtransient-addscoped-and-addsingleton-services-differences
// Add as singleton to enhance performance
services.AddSingleton<IMyApp>(serviceApp);

// Bridge
services.Configure<BridgeOptions>(configuration.GetSection(BridgeOptions.SectionName));
services.AddHttpClient<IBridgeProxy, BridgeProxy>();

// Storage
var storageSection = serviceApp.Section.GetSection("Storage");
services.AddSingleton<IStorage>((provider) =>
{
    var context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var root = storageSection.GetValue<string?>("Root") ?? "./Resources";

    if (root.StartsWith("./"))
    {
        var host = provider.GetRequiredService<IWebHostEnvironment>();
        root = Path.Combine(host.ContentRootPath, root[2..]);
    }

    Log.Logger.Debug("Storage root is {root}", root);

    var urlRoot = storageSection.GetValue<string>("URLRoot")
        ?? (context == null ? throw new Exception("No HttpContext") : $"{context.Request.Scheme}://{context.Request.Host}/api/Storage");
    return new LocalStorage(root, urlRoot);
});

// Business services
services.AddScoped<IServiceUserAccessor, ServiceUserAccessor>();
services.AddScoped<IArticleService, ArticleService>();
services.AddScoped<ITabService, TabService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IExternalService, ExternalService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IWebsiteService, WebsiteService>();
services.AddScoped<IPublicService, PublicService>();

services.AddControllers().AddJsonOptions(configure =>
{
    // Configure and hold the default options
    serviceApp.DefaultJsonSerializerOptions = configure.JsonSerializerOptions;
});

// Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
    c.SwaggerDoc("v1", new() { Title = "ETSOO Website CMS / 亿速思维网站内容管理系统", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,
          },
          new List<string>()
        }
    });
});

// Add to support access HttpContext
services.AddHttpContextAccessor();

// Add services to the container.
services.AddControllersWithViews();

// Configue compression
// https://gunnarpeipman.com/aspnet-core-compress-gzip-brotli-content-encoding/
services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
});

// Configue CORS
var corsOptions = new CorsPolicySetupOptions(Cors, builder.Environment.IsDevelopment())
{
    ExposedHeaders = new[] { Constants.RefreshTokenHeaderName }
};

if (corsOptions.Required)
{
    services.AddCors(options =>
    {
        // Add default policy
        // Or AddPolicy with a specific policy
        options.AddDefaultPolicy(builder => builder.Setup(corsOptions));
    });
}

// Global settings
ActionResult.UtcDateTime = true;

// Request localization setup
// Use Content-Language Header for culture detection
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-5.0
// https://www.jerriepelser.com/blog/how-aspnet5-determines-culture-info-for-localization/
var localizationOptions = new RequestLocalizationOptions
{
    ApplyCurrentCultureToResponseHeaders = true,
    RequestCultureProviders = new IRequestCultureProvider[] {
                    new QueryStringRequestCultureProvider(),
                    new ContentLanguageHeaderRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                }
}.SetDefaultCulture(Cultures[0])
    .AddSupportedCultures(Cultures)
    .AddSupportedUICultures(Cultures);

var app = builder.Build();

app.UseIpRateLimiting();
app.UseForwardedHeaders();

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    // https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0
    app.UseDeveloperExceptionPage();

    app.UseSwagger();

    // Change "/swagger/v1/swagger.json" to "./v1/swagger.json" under IIS
    app.UseSwaggerUI(c => c.SwaggerEndpoint("./v1/swagger.json", "V1"));
}
else
{
    if (Environment.GetEnvironmentVariable("SSLOnly") != "false")
    {
        // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-2.1&tabs=visual-studio
        app.UseHsts();

        app.UseHttpsRedirection();
    }

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(context =>
        {
            // Return without details but status
            context.Response.ContentType = "application/json";
            return context.Response.CompleteAsync();
        });
    });
}

// Configurable request logging
if (RequestLogging)
{
    app.UseSerilogRequestLogging();
}

app.UseStaticFiles();
app.UseRouting();

// Enable CORS (Cross-Origin Requests)
// The call to UseCors must be placed after UseRouting, but before UseAuthorization
if (corsOptions.Required)
{
    app.UseCors();
}

// Enable compression
app.UseResponseCompression();

// Authentication is the process of validating user credentials.
// Authorization is the process of checking privileges for a user to access specific modules.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.MapFallbackToFile("index.html");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, $"Host at {Environment.MachineName} terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}