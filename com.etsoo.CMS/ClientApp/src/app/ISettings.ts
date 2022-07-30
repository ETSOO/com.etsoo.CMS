/**
 * App settings
 */
export interface ISettings {
  /**
   * External account required
   */
  externalAccountRequired?: boolean;

  /**
   * Leader approval required
   */
  leaderApprovalRequired?: boolean;

  /**
   * Finance write off required
   */
  writeOffRequired?: boolean;

  /**
   * Support foreign currency
   */
  supportForeignCurrency?: boolean;
}
