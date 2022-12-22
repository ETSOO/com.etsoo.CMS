/**
 * Audit kind
 * Defs/AuditKind.cs
 */
export enum AuditKind {
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
  UpdateArticle,
  CreateResource
}
