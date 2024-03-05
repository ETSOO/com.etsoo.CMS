import { WebsiteJsonData } from './WebsiteJsonData';

export type SettingsUpdateDto = {
  domain: string;
  rootUrl?: string;
  title: string;
  description?: string;
  keywords?: string;
  jsonData?: string | WebsiteJsonData;
};
