export type UserDto = {
  /**
   * Id
   */
  id: string;

  /**
   * Role
   */
  role: number;

  /**
   * status
   */
  status: number;

  /**
   * Is self
   */
  isSelf: boolean;

  /**
   * Creation
   */
  creation: Date;

  /**
   * Refresh time
   */
  refreshTime?: Date;
};
