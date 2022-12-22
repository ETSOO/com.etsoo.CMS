import { ArticleLink } from './ArticleLink';

export type ArticleQueryDto = ArticleLink & {
  /**
   * Id
   */
  id: number;

  /**
   * Creation date
   */
  creation: Date | string;

  /**
   * Is the author
   */
  isSelf: boolean;

  /**
   * Title
   */
  title: string;

  /**
   * Tab name 1
   */
  tabName1: string;

  /**
   * Tab name 2
   */
  tabName2?: string;

  /**
   * Tab name 3
   */
  tabName3?: string;
};
