export type Account = {
  /**
   * Id
   */
  id: number;

  /**
   * Display name
   */
  displayName: string;

  /**
   * Account bank
   */
  accountBank: string;

  /**
   * Account name (beneficiary)
   */
  accountName: string;

  /**
   * Account number
   */
  accountNumber: string;

  /**
   * Balance
   */
  accountBalance?: number;

  /**
   * Is external account
   */
  externalAccount: boolean;

  /**
   * Balance editable
   */
  balanceEditable: boolean;

  /**
   * Is system account
   */
  isSystem: boolean;

  /**
   * Enabled or not
   */
  enabled: boolean;

  /**
   * Entity status
   */
  entityStatus: number;

  /**
   * Creation
   */
  creation: Date;

  /**
   * No bank data
   */
  none?: boolean;
};
