import { EntityApi, IApiPayload, ResultPayload } from "@etsoo/appscript";
import { ReactAppType } from "@etsoo/materialui";
import { DataTypes } from "@etsoo/shared";
import { DashboardDto } from "./dto/website/DashboardDto";
import { PluginDto } from "./dto/website/PluginDto";
import { PluginUpdateDto } from "./dto/website/PluginUpdateDto";
import { PluginViewDto } from "./dto/website/PluginViewDto";
import { ResourceDto } from "./dto/website/ResourceDto";
import { SettingsUpdateDto } from "./dto/website/SettingsUpdateDto";
import { WebsiteJsonData } from "./dto/website/WebsiteJsonData";

/**
 * Website API
 */
export class WebsiteApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super("Website", app);
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
      "Website/CreateOrUpdateResource",
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
    return this.api.put("Website/CreateService", rq, payload);
  }

  /**
   * Get dashboard data
   * @param payload Payload
   * @returns Result
   */
  dashboard(payload?: IApiPayload<DashboardDto>) {
    return this.api.get<DashboardDto>("Website/Dashboard", undefined, payload);
  }

  /**
   * Get mobile base64 QRCode
   * @param payload Payload
   */
  qrCode(payload?: IApiPayload<string>) {
    return this.api.get("Website/QRCode", undefined, payload);
  }

  /**
   * Query resources
   * @param payload Payload
   * @returns Result
   */
  queryResources(payload?: IApiPayload<ResourceDto[]>) {
    return this.api.post("Website/QueryResources", undefined, payload);
  }

  /**
   * Query article JSON data schema
   * @param payload Payload
   * @returns Result
   */
  queryArticleJsonDataSchema(payload?: IApiPayload<string>) {
    return this.api.get(
      "Website/QueryArticleJsonDataSchema",
      undefined,
      payload
    );
  }

  /**
   * Query tab JSON data schema
   * @param payload Payload
   * @returns Result
   */
  queryTabJsonDataSchema(payload?: IApiPayload<string>) {
    return this.api.get("Website/QueryTabJsonDataSchema", undefined, payload);
  }

  /**
   * Query services (plugins)
   * @param payload Payload
   * @returns Result
   */
  queryServices(payload?: IApiPayload<PluginDto[]>) {
    return this.api.post("Website/QueryServices", undefined, payload);
  }

  /**
   * Read JSON data
   * @param payload Payload
   * @returns Result
   */
  async readJsonData(payload?: IApiPayload<{ jsonData?: WebsiteJsonData }>) {
    const result = await this.api.get(
      "Website/readJsonData",
      undefined,
      payload
    );
    if (result == null) return;
    return result.jsonData ?? {};
  }

  /**
   * Read service (plugin)
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  readService(id: string, payload?: IApiPayload<PluginViewDto>) {
    return this.api.get(`Website/ReadService/${id}`, undefined, payload);
  }

  /**
   * Regenerate all tab URLs
   * @param payload Payload
   * @returns Result
   */
  regenerateTabUrls(payload?: ResultPayload) {
    return this.api.put("Website/RegenerateTabUrls", undefined, payload);
  }

  /**
   * Regenerate URLs
   * @param urls URLs
   * @param payload Payload
   * @returns Result
   */
  regenerateUrls(urls: string[], payload?: ResultPayload) {
    return this.api.post("Website/RegenerateUrl", urls, {
      contentType: this.api.jsonContentType,
      ...payload
    });
  }

  /**
   * Read settings
   * @param payload Payload
   * @returns Result
   */
  readSettings(payload?: IApiPayload<SettingsUpdateDto>) {
    return this.api.get("Website/ReadSettings", undefined, payload);
  }

  /**
   * Update resource URL
   * @param oldResourceUrl Old resource URL
   * @param payload Payload
   * @returns Result
   */
  updateResourceUrl(oldResourceUrl: string, payload?: ResultPayload) {
    return this.api.put(
      "Website/UpdateResourceUrl",
      { oldResourceUrl },
      payload
    );
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
    return this.api.put("Website/UpdateService", rq, payload);
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
    return this.api.put("Website/UpdateSettings", rq, payload);
  }
}
