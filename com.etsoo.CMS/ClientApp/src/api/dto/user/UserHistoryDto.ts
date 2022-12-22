import { AuditLineChangesDto } from '@etsoo/appscript';
import { AuditFlag } from './AuditFlag';

export type UserHistoryDto = {
  /**
   * Id
   */
  id: number;

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
