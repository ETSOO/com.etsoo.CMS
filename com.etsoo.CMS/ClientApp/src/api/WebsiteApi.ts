import { EntityApi, IApiPayload, ResultPayload } from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { DataTypes } from '@etsoo/shared';
import { DashboardDto } from './dto/website/DashboardDto';
import { PluginDto } from './dto/website/PluginDto';
import { PluginUpdateDto } from './dto/website/PluginUpdateDto';
import { ResourceDto } from './dto/website/ResourceDto';
import { SettingsUpdateDto } from './dto/website/SettingsUpdateDto';

/**
 * Website API
 */
export class WebsiteApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('Website', app);
  }

  /**
   * Create or update resource
   * @param id Id
   * @param value Value
   * @param payload Payload
   * @returns Result
   */
  createOrUpdateResource(id: string, value: string, payload?: ResultPayload) {
    return this.api.put(
      'Website/CreateOrUpdateResource',
      { id, value },
      payload
    );
  }

  /**
   * Create service
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  createService(
    rq: DataTypes.AddOrEditType<PluginUpdateDto, false>,
    payload?: ResultPayload
  ) {
    return this.api.put('Website/CreateService', rq, payload);
  }

  /**
   * Get dashboard data
   * @param payload Payload
   * @returns Result
   */
  dashboard(payload?: IApiPayload<DashboardDto>) {
    return this.api.get<DashboardDto>('Website/Dashboard', undefined, payload);
  }

  /**
   * Query resources
   * @param payload Payload
   * @returns Result
   */
  queryResources(payload?: IApiPayload<ResourceDto[]>) {
    return this.api.post('Website/QueryResources', undefined, payload);
  }

  /**
   * Query services (plugins)
   * @param payload Payload
   * @returns Result
   */
  queryServices(payload?: IApiPayload<PluginDto[]>) {
    return this.api.post('Website/QueryServices', undefined, payload);
  }

  /**
   * Read settings
   * @param payload Payload
   * @returns Result
   */
  readSettings(payload?: IApiPayload<SettingsUpdateDto>) {
    return this.api.get('Website/ReadSettings', undefined, payload);
  }

  /**
   * UPdate service
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  updateService(
    rq: DataTypes.AddOrEditType<PluginUpdateDto, true>,
    payload?: ResultPayload
  ) {
    return this.api.put('Website/UpdateService', rq, payload);
  }

  /**
   * Update settings
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  updateSettings(
    rq: DataTypes.AddOrEditType<SettingsUpdateDto, true, never>,
    payload?: ResultPayload
  ) {
    return this.api.put('Website/UpdateSettings', rq, payload);
  }
}
