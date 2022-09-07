import { AuditLineUpdateData } from '@etsoo/materialui';
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
  content?: string | { auditData?: AuditLineUpdateData };

  /**
   * Audit data
   */
  auditData?: AuditLineUpdateData;

  /**
   * Flag
   */
  flag: AuditFlag;
};
