import { CommonPage, useParamsEx } from '@etsoo/react';
import { Divider, Typography } from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import {
  HistoryAccount,
  HistoryDisplay,
  HistoryLine
} from '../../components/HistoryDisplay';

interface HistoryResults {
  data1: HistoryAccount | null;
  data2: HistoryLine[] | null;
}

function AccountHistory() {
  // Route
  const { id: accountId = 0 } = useParamsEx({ id: 'number' });

  // Account state
  const [account, setAccount] = React.useState<HistoryAccount>();

  React.useEffect(() => {
    // Page title
    app.setPageKey('accountHistory');

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <CommonPage>
      <HistoryDisplay
        headerTitle={
          account == null ? undefined : (
            <React.Fragment>
              <Typography>
                {(account.externalAccount ? account.accountName + ', ' : '') +
                  account.accountBank +
                  ', ' +
                  account.accountNumber}
              </Typography>
              <Divider />
            </React.Fragment>
          )
        }
        formatAmount={(data) => {
          const p = account?.externalAccount !== data.isIncome;
          return [p, app.formatMoney(p ? data.amount : -data.amount)];
        }}
        loadData={async (rq) => {
          const result = await app.serviceApi.post<HistoryResults>(
            'AccountLine/QueryAccountHistory',
            { accountId, ...rq },
            {
              showLoading: false
            }
          );
          if (result) {
            if (result.data1) setAccount(result.data1);
            return result.data2 ?? [];
          }
          return undefined;
        }}
      />
    </CommonPage>
  );
}

export default AccountHistory;
