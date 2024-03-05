import { IApiPayload, AuthApi as AuthApiBase, LoginRQ } from '@etsoo/appscript';
import { ServiceLoginResult } from '@etsoo/materialui';

/**
 * Authentication API
 */
export class AuthApi extends AuthApiBase {
  /**
   * Login
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  login(rq: LoginRQ, payload?: IApiPayload<ServiceLoginResult>) {
    return this.loginBase(rq, payload);
  }
}
