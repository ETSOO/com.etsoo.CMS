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
   * Refresh time
   */
  refreshTime?: Date;

  /**
   * enabled (status)
   */
  enabled: boolean;
};
