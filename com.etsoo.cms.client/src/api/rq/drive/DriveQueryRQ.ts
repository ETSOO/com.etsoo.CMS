import { QueryRQ } from "@etsoo/appscript";

/**
 * Online drive request data
 */
export type DriveQueryRQ = QueryRQ & {
  name?: string;
  author?: string;
  creationStart?: string | Date;
  creationEnd?: string | Date;
};
