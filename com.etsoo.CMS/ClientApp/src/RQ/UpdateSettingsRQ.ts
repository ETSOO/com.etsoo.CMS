import { ISettings } from '../app/ISettings';

/**
 * Update settings request data
 */
export type UpdateSettingsRQ = ISettings & {
  /**
   * Changed fields
   */
  changedFields?: string[];
};
