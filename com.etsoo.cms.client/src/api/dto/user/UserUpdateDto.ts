export type UserUpdateDto = {
  /**
   * Id
   */
  id: string;

  /**
   * Role
   */
  role: number;

  /**
   * Password
   */
  password: string;

  /**
   * Refresh time
   */
  refreshTime?: Date;

  /**
   * enabled (status)
   */
  enabled: boolean;
};
