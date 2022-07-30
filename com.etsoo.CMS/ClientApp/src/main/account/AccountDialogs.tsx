import { BusinessUtils, IActionResult, IdLabelDto } from '@etsoo/appscript';
import { ComboBox, SelectEx, Tiplist, VBox } from '@etsoo/react';
import { DomUtils } from '@etsoo/shared';
import { TextField } from '@mui/material';
import { app } from '../../app/MyApp';
import { Account } from '../../dto/Account';
import { AccountLineView } from '../../dto/AccountLineView';

/**
 * Account dialogs
 */
export namespace AccountDialogs {
  /**
   * Approve
   * @param isApproval Is lead approval operation
   */
  export function Approve(
    data: AccountLineView,
    isApproval: boolean,
    callback: () => PromiseLike<void>
  ) {
    // Labels
    const labels = app.getLabels(
      'leaderApproval',
      'leaderApprovalConfirm',
      'writeOff',
      'receivingAccount',
      'paymentAccount'
    );

    if (isApproval && app.serviceUser?.writeOffRequired) {
      // Simple confirm
      app.notifier.confirm(
        labels.leaderApprovalConfirm,
        labels.leaderApproval,
        async (success) => {
          if (!success) return;

          // Submit
          const result = await app.serviceApi.put<IActionResult>(
            `AccountLine/Approve/${data.id}`,
            undefined,
            {
              showLoading: false // default will show the loading bar and cause the dialog closed
            }
          );
          if (result == null) return;

          if (result.ok) {
            await callback();
            return;
          }

          app.alertResult(result);
        }
      );
      return;
    }

    // Company account and external account label
    const companyAccountLabel = data.isCollection
      ? labels.receivingAccount
      : labels.paymentAccount;
    const externalAccountLabel = data.isCollection
      ? labels.paymentAccount
      : labels.receivingAccount;

    // Confirm bank accounts
    app.showInputDialog({
      title: isApproval ? labels.leaderApproval : labels.writeOff,
      message: '',
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const formData = DomUtils.dataAs(new FormData(form), {
          companyAccount: 'number',
          externalAccount: 'number'
        });

        if (formData.companyAccount == null) {
          DomUtils.setFocus('companyAccountInput', form);
          return false;
        }

        if (
          app.serviceUser?.externalAccountRequired &&
          formData.externalAccount == null
        ) {
          DomUtils.setFocus('externalAccountInput', form);
          return false;
        }

        // Request data
        const rq = { id: data.id, ...formData };

        // Submit
        const result = await app.serviceApi.put<IActionResult>(
          'AccountLine/WriteOff',
          rq,
          {
            showLoading: false // default will show the loading bar and cause the dialog closed
          }
        );
        if (result == null) return;

        if (result.ok) {
          await callback();
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: (
        <VBox gap={3} marginTop={1.5}>
          <ComboBox<Account>
            name="companyAccount"
            loadData={async () =>
              await app.serviceApi.get<Account[]>('Account/CompanyAccounts')
            }
            label={companyAccountLabel}
            getOptionLabel={(option) =>
              option.accountBank + ', ' + option.accountNumber
            }
            idValue={data.companyAccount}
            inputRequired
          />
          <Tiplist
            label={externalAccountLabel}
            name="externalAccount"
            idValue={data.externalAccount}
            loadData={async (keyword, id) => {
              return await app.serviceApi.post<IdLabelDto[]>(
                'Account/List',
                {
                  id,
                  keyword
                },
                { defaultValue: [], showLoading: false }
              );
            }}
            inputRequired={app.serviceUser?.externalAccountRequired}
          />
        </VBox>
      )
    });
  }

  /**
   * Create cash account
   */
  export function createCashAccount(callback: () => void) {
    // Labels
    const labels = app.getLabels(
      'addCashAccount',
      'currency',
      'accountBalance',
      'CNY'
    );

    // Currencies
    const currencies = BusinessUtils.getCurrencies(['CNY'], app.labelDelegate);

    app.showInputDialog({
      title: labels.addCashAccount,
      message: '',
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const data = DomUtils.dataAs(new FormData(form), {
          currency: 'string',
          accountName: 'string',
          accountBalance: 'number'
        });

        // Organization
        if (data.currency == null) {
          DomUtils.setFocus('currency', form);
          return false;
        }

        if (data.accountBalance == null) {
          DomUtils.setFocus('accountBalance', form);
          return false;
        }

        // Request data
        const rq = {
          currency: data.currency,
          accountBalance: data.accountBalance,
          accountName: currencies.find((c) => c.id === data.currency)?.label
        };

        // Submit
        const result = await app.serviceApi.post<IActionResult>(
          'Account/CreateCashAccount',
          rq,
          {
            showLoading: false // default will show the loading bar and cause the dialog closed
          }
        );
        if (result == null) return;

        if (result.ok) {
          callback();
          return;
        }

        app.alertResult(result);
      },
      inputs: (
        <VBox marginTop={2}>
          <SelectEx
            name="currency"
            variant="standard"
            margin="dense"
            options={currencies}
            label={labels.currency}
            value="CNY"
            fullWidth
            required
          />
          <TextField
            name="accountBalance"
            margin="dense"
            variant="standard"
            type="number"
            label={labels.accountBalance}
            required
            fullWidth
          />
        </VBox>
      )
    });
  }
}
