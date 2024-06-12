import { TabLayout } from "./TabLayout";

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
   * Layout
   */
  layout: TabLayout;

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
