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
        ChangePassword
    }
}
