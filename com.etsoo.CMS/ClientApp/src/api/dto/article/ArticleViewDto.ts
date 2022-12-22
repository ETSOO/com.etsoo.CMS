import { ArticleLink } from './ArticleLink';

export type ArticleViewDto = ArticleLink & {
  id: number;
  title: string;
  subtitle?: string;
  description?: string;
  keywords?: string;
  author: string;
  creation: Date | string;
  release: Date | string;
  status: number;
  weight: number;
  tabName1: string;
  tabName2?: string;
  tabName3?: string;
};
