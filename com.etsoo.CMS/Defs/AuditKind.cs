namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Audit kind
    /// 审计类型
    /// </summary>
    public enum AuditKind : byte
    {
        Init,
        Login,
        TokenLogin,
        ChangePassword,
        CreateUser,
        UpdateUser,
        ResetUserPassword,
        UpdateWebsiteSettings,
        CreateService,
        UpdateService,
        CreateTab,
        UpdateTab,
        CreateArticle,
        UpdateArticle
    }
}
