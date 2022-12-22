export type TabUpdateDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Parent tab
   */
  parent?: number;

  /**
   * Layout
   */
  layout: number;

  /**
   * Name
   */
  name: string;

  /**
   * URL
   */
  url: string;

  /**
   * Enabled
   */
  enabled: boolean;

  /**
   * Articles
   */
  articles: number;
};
