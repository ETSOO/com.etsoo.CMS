/**
 * Online drive query data
 */
export type DriveQueryDto = {
  /**
   * Id
   */
  id: string;

  /**
   * File name
   */
  name: string;

  /**
   * File size
   */
  size: number;

  /**
   * Author
   */
  author: string;

  /**
   * Shared or not
   */
  shared: boolean;

  /**
   * Creation time
   */
  creation: Date | string;
};
