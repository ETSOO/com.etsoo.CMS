export type PlugEditDto = {
  /**
   * App id
   */
  app: string;

  /**
   * Secret
   */
  secret?: string;
};

export type PluginDto = {
  /**
   * Id
   */
  id: string;

  /**
   * App id
   */
  app: string;

  /**
   * Enabled
   */
  enabled: boolean;

  /**
   * Refresh time
   */
  refreshTime?: Date;
};
