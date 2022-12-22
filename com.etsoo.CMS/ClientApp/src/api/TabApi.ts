import { EntityApi, IApiPayload, IdResultPayload } from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { DataTypes } from '@etsoo/shared';
import { TabDto } from './dto/tab/TabDto';
import { TabUpdateDto } from './dto/tab/TabUpdateDto';
import { TabQueryRQ } from './rq/tab/TabQueryRQ';

/**
 * Tab API
 */
export class TabApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('Tab', app);
  }

  /**
   * Read tab's ancestor
   * @param tab Tab id
   * @param payload Payload
   * @returns Result
   */
  ancestorRead(tab: number, payload?: IApiPayload<number[]>) {
    return this.api.get('Tab/AncestorRead/' + tab, undefined, payload);
  }

  /**
   * Create
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  create(
    rq: DataTypes.AddOrEditType<TabUpdateDto, false>,
    payload?: IdResultPayload
  ) {
    return this.createBase(rq, payload);
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: TabQueryRQ, payload?: IApiPayload<TabDto[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Sort
   * @param items Items
   * @param payload Payload
   * @returns Result
   */
  sort(items: { id: number }[], payload?: IApiPayload<number>) {
    return this.sortBase(items, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(
    rq: DataTypes.AddOrEditType<TabUpdateDto, true>,
    payload?: IdResultPayload
  ) {
    return this.updateBase(rq, payload);
  }

  /**
   * Read for update
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  updateRead(id: number, payload?: IApiPayload<TabUpdateDto>) {
    return super.updateReadBase(id, payload);
  }
}
