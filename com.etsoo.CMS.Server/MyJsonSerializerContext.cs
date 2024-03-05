using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Article;
using com.etsoo.CMS.RQ.Drive;
using com.etsoo.CMS.RQ.Public;
using com.etsoo.CMS.RQ.Service;
using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CMS.RQ.User;
using com.etsoo.CMS.RQ.Website;
using com.etsoo.WeiXin.RQ;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace com.etsoo.CMS
{
    /// <summary>
    /// JSON serializer context
    /// JSON 序列化器上下文
    /// </summary>
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    )]
    [JsonSerializable(typeof(ArticleCreateRQ))]
    [JsonSerializable(typeof(ArticleDeletePhotoRQ))]
    [JsonSerializable(typeof(ArticleQueryRQ))]
    [JsonSerializable(typeof(ArticleSortPhotosRQ))]
    [JsonSerializable(typeof(ArticleUpdateRQ))]
    [JsonSerializable(typeof(ArticleUpdatePhotoRQ))]
    [JsonSerializable(typeof(CreateJsApiSignatureRQ))]
    [JsonSerializable(typeof(DbArticleQuery[]))]
    [JsonSerializable(typeof(DriveCreateRQ))]
    [JsonSerializable(typeof(DriveQueryRQ))]
    [JsonSerializable(typeof(DriveShareFileRQ))]
    [JsonSerializable(typeof(DriveUpdateRQ))]
    [JsonSerializable(typeof(GetArticleRQ))]
    [JsonSerializable(typeof(GetArticlesRQ))]
    [JsonSerializable(typeof(IEnumerable<GalleryPhotoDto>))]
    [JsonSerializable(typeof(InitializeRQ))]
    [JsonSerializable(typeof(LoginDto))]
    [JsonSerializable(typeof(JsonDataGalleryLogoSize))]
    [JsonSerializable(typeof(ResourceCreateRQ))]
    [JsonSerializable(typeof(ServiceCreateRQ))]
    [JsonSerializable(typeof(ServiceUpdateRQ))]
    [JsonSerializable(typeof(SendEmailRQ))]
    [JsonSerializable(typeof(TabCreateRQ))]
    [JsonSerializable(typeof(TabQueryRQ))]
    [JsonSerializable(typeof(TranslateRQ))]
    [JsonSerializable(typeof(Dictionary<int, short>))]
    [JsonSerializable(typeof(TabUpdateRQ))]
    [JsonSerializable(typeof(UserCreateRQ))]
    [JsonSerializable(typeof(UserHistoryQueryRQ))]
    [JsonSerializable(typeof(UserQueryRQ))]
    [JsonSerializable(typeof(UserResetPasswordRQ))]
    [JsonSerializable(typeof(UserUpdateRQ))]
    [JsonSerializable(typeof(WebsiteUpdateResurceUrlRQ))]
    [JsonSerializable(typeof(WebsiteUpdateSettingsRQ))]

    // Framework models
    [JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(ValidationProblemDetails))]
    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
