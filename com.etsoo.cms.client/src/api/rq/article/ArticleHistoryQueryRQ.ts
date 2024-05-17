import { QueryRQ } from "@etsoo/appscript";

export type ArticleHistoryQueryRQ = QueryRQ & {
  target: number;
  creationStart?: string | Date;
  creationEnd?: string | Date;
};
