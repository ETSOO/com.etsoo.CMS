import { CommonPage, useParamsEx } from '@etsoo/react';
import { Divider, Typography } from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import {
  HistoryDisplay,
  HistoryLine,
  HistorySubject
} from '../../components/HistoryDisplay';

interface HistoryResults {
  data1: HistorySubject | null;
  data2: HistoryLine[] | null;
}

function SubjectHistory() {
  // Route
  const { id: subjectId = 0 } = useParamsEx({ id: 'number' });

  // Subject state
  const [subject, setSubject] = React.useState<HistorySubject>();

  React.useEffect(() => {
    // Page title
    app.setPageKey('subjectHistory');

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <CommonPage>
      <HistoryDisplay
        headerTitle={
          subject == null ? undefined : (
            <React.Fragment>
              <Typography>{subject.name}</Typography>
              <Divider />
            </React.Fragment>
          )
        }
        formatAmount={(data) => {
          return [data.amount >= 0, app.formatMoney(data.amount)];
        }}
        loadData={async (rq) => {
          const result = await app.serviceApi.post<HistoryResults>(
            'AccountLine/QuerySubjectHistory',
            { subjectId, ...rq },
            {
              showLoading: false
            }
          );
          if (result) {
            if (result.data1) setSubject(result.data1);
            return result.data2 ?? [];
          }
          return undefined;
        }}
      />
    </CommonPage>
  );
}

export default SubjectHistory;
