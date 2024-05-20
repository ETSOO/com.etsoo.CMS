import { QueryRQ } from "@etsoo/appscript";

export type HistoryQueryRQ = QueryRQ & {
  author?: string;
  kind?: number;
  creationStart?: string | Date;
  creationEnd?: string | Date;
};
