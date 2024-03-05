export type PluginUpdateDto = {
  /**
   * Device id
   */
  deviceId: string;

  /**
   * Id
   */
  id: string;

  /**
   * App id
   */
  app: string;

  /**
   * App secret
   */
  secret?: string;

  /**
   * Enabled
   */
  enabled: boolean;
};
