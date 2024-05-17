import { AuditLineChangesDto } from "@etsoo/appscript";
import { AuditFlag } from "../user/AuditFlag";

export type ArticleHistoryDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Author
   */
  author: string;

  /**
   * Creation
   */
  creation: Date;

  /**
   * Title
   */
  title: string;

  /**
   * Content
   */
  content?: string | { auditData?: AuditLineChangesDto };

  /**
   * Audit data
   */
  auditData?: AuditLineChangesDto;

  /**
   * Flag
   */
  flag: AuditFlag;
};
