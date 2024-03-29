export type TabDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Name
   */
  name: string;

  /**
   * Url
   */
  url: string;

  /**
   * Logo
   */
  logo?: string;

  /**
   * Level
   */
  level?: number;

  /**
   * Parent tab
   */
  parent?: number;

  /**
   * Articles
   */
  articles: number;
};
