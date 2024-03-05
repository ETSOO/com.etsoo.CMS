namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Audit kind
    /// 审计类型
    /// </summary>
    public enum AuditKind : byte
    {
        Init = 0,
        Login = 1,
        TokenLogin = 2,
        ChangePassword = 3,
        CreateUser = 4,
        UpdateUser = 5,
        ResetUserPassword = 6,
        UpdateWebsiteSettings = 7,
        CreateService = 8,
        UpdateService = 9,
        CreateTab = 10,
        UpdateTab = 11,
        CreateArticle = 12,
        UpdateArticle = 13,
        CreateResource = 14,
        UpdateTabLogo = 15,
        UpdateArticleLogo = 16,
        UpdateGallery = 17,
        DeleteGalleryPhoto = 18,
        SortGalleryPhoto = 19,
        UpdateResurceUrl = 20,
        UpdateGalleryItem = 21,
        OnlineDrive = 22,
        DeleteArticle = 23
    }
}
