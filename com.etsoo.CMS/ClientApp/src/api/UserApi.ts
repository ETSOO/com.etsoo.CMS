import {
  EntityApi,
  IApiPayload,
  IdResultPayload,
  ResultPayload
} from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { DataTypes, IActionResult } from '@etsoo/shared';
import { UserDto } from './dto/user/UserDto';
import { UserHistoryDto } from './dto/user/UserHistoryDto';
import { UserUpdateDto } from './dto/user/UserUpdateDto';
import { HistoryQueryRQ } from './rq/user/HistoryQueryRQ';
import { UserQueryRQ } from './rq/user/UserQueryRQ';

/**
 * User API
 */
export class UserApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('User', app);
  }

  /**
   * Change password
   * @param oldPassword Ole password
   * @param password New password
   * @param payload Payload
   * @returns Result
   */
  changePassword(
    oldPassword: string,
    password: string,
    payload?: ResultPayload
  ) {
    const rq = {
      deviceId: this.app.deviceId,
      oldPassword: this.app.encrypt(this.app.hash(oldPassword)),
      password: this.app.encrypt(this.app.hash(password))
    };
    return this.api.put('User/ChangePassword', rq, payload);
  }

  /**
   * Create
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  create(
    rq: DataTypes.AddOrEditType<UserUpdateDto, false>,
    payload?: IdResultPayload
  ) {
    return this.createBase(rq, payload);
  }

  /**
   * Delete
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  delete(id: string, payload?: ResultPayload) {
    return this.deleteBase(id, payload);
  }

  /**
   * Operation history
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  history(rq: HistoryQueryRQ, payload?: IApiPayload<UserHistoryDto[]>) {
    return this.api.post('User/History', rq, payload);
  }

  /**
   * Reset password
   * @param id User id
   * @param payload Payload
   * @returns Result
   */
  resetPassword(
    id: string,
    payload?: IApiPayload<IActionResult<{ password: string }>>
  ) {
    return this.api.put(
      'User/ResetPassword/',
      { deviceId: this.app.deviceId, id },
      payload
    );
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: UserQueryRQ, payload?: IApiPayload<UserDto[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(
    rq: DataTypes.AddOrEditType<UserUpdateDto, true>,
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
  updateRead(id: string, payload?: IApiPayload<UserUpdateDto>) {
    return super.updateReadBase(id, payload);
  }
}
