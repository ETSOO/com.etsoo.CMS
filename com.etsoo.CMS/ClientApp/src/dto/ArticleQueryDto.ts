export type ArticleQueryDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Creation date
   */
  creation: Date;

  /**
   * Is the author
   */
  isSelf: boolean;

  /**
   * Title
   */
  title: string;

  /**
   * Tab
   */
  tab: string;
};
