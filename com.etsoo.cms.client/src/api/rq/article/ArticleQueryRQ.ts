import { QueryRQ } from "@etsoo/appscript";

export type ArticleQueryRQ = QueryRQ & {
  title?: string;
  tab?: number;
};
