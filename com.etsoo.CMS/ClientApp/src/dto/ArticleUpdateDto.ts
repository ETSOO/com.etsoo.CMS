import { EntityStatus } from '@etsoo/appscript';

export type ArticleUpdateDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Tab 1
   */
  tab1: number;

  /**
   * Title
   */
  title: string;

  /**
   * Subtitle
   */
  subtitle?: string;

  /**
   * Content
   */
  content: string;

  /**
   * Keywords
   */
  keywords?: string;

  /**
   * Description
   */
  description?: string;

  /**
   * URL
   */
  url?: string;

  /**
   * Logo
   */
  logo?: string;

  /**
   * Slideshow
   */
  slideshow?: string;

  /**
   * Release datetime
   */
  release: Date | string;

  /**
   * Status
   */
  status: EntityStatus;
};
