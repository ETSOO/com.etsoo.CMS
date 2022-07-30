import { IServiceUser } from '@etsoo/react';
import { ISettings } from './ISettings';

/**
 * My service user
 */
export interface MyUser extends IServiceUser, ISettings {
  /**
   * Organization name
   */
  organizationName: string;
}
