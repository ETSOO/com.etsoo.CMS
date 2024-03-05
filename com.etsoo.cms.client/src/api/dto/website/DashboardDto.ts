export type DashboardDto = {
  site?: {
    domain?: string;
    version?: string;
  };
  articles: {
    id: number;
    title: string;
    author: string;
    refreshTime: string | Date;
  }[];
  audits: {
    rowid: number;
    title: string;
    creation: string | Date;
    author: string;
  }[];
};
