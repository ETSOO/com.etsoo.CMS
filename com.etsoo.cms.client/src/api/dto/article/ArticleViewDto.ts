import { ArticleLink } from "./ArticleLink";

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
  logo?: string;
  jsonData?: unknown;
};

export type ArticleViewHistoryDto = {
  rowid: number;
  title: string;
  creation: string | Date;
  author: string;
};

export type ArticleViewAllDto = {
  data: ArticleViewDto;
  audits: ArticleViewHistoryDto[];
};
