import { EntityStatus, UserRole } from '@etsoo/appscript';
import {
  AuditDisplay,
  AuditLine,
  ButtonLink,
  GridDataType,
  useParamsEx,
  ViewPage
} from '@etsoo/react';
import React from 'react';
import { app } from '../../app/MyApp';
import { AccountLineView } from '../../dto/AccountLineView';
import EditIcon from '@mui/icons-material/Edit';
import ApprovalIcon from '@mui/icons-material/Approval';
import CheckIcon from '@mui/icons-material/Check';
import { Button } from '@mui/material';
import { AccountDialogs } from './AccountDialogs';

function ViewLine() {
  // Route
  const { id } = useParamsEx({ id: 'number' });

  // Labels
  const labels = app.getLabels(
    'edit',
    'leaderApproval',
    'writeOff',
    'income',
    'expense'
  );

  // Has edit permission
  const hasEditPermisson = (item: AccountLineView) => {
    if (item.entityStatus < EntityStatus.Completed) {
      if (
        (item.selfLine && item.entityStatus === EntityStatus.Normal) ||
        app.hasPermission([UserRole.Finance, UserRole.Admin, UserRole.Founder])
      )
        return true;
    }

    return false;
  };

  // Has approval permission
  const hasApprovalPermission = (item: AccountLineView) => {
    if (
      item.entityStatus === EntityStatus.Normal &&
      app.serviceUser?.leaderApprovalRequired &&
      app.hasPermission([UserRole.Admin, UserRole.Founder])
    ) {
      return true;
    }

    return false;
  };

  // Has writeoff permission
  const hasWriteoffPermission = (item: AccountLineView) => {
    if (
      (item.entityStatus === EntityStatus.Approved ||
        (item.entityStatus === EntityStatus.Normal &&
          !app.serviceUser?.leaderApprovalRequired)) &&
      app.hasPermission(UserRole.Finance)
    ) {
      return true;
    }

    return false;
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('viewLine');
  }, []);

  return (
    <ViewPage<AccountLineView>
      fields={[
        { data: 'title', singleRow: true },
        {
          data: (item) =>
            item.subject +
            ', ' +
            (item.kind === 'i' ? labels.income : labels.expense),
          label: 'subject',
          singleRow: false
        },
        { data: 'user', label: 'applicant' },
        ['amount', GridDataType.Money],
        {
          data: (item) => app.getUnitLabel(item.repeat, true),
          label: 'repeat'
        },
        ['happenDate', GridDataType.Date],
        { data: (item) => app.getEntityStatusLabel(item), label: 'status' },
        { data: (item) => item.leader, label: 'leaderApproval' },
        { data: (item) => item.accountant, label: 'writeOff' },
        {
          data: (item) =>
            item.accountBank == null
              ? undefined
              : item.accountBank + ', ' + item.accountNumber,
          label: 'companyAccount',
          singleRow: true
        },
        {
          data: (item) =>
            item.externalAccountName == null
              ? undefined
              : item.externalAccountName +
                ', ' +
                item.externalAccountBank +
                ', ' +
                item.externalAccountNumber,
          label: 'companyAccount',
          singleRow: true
        },
        ['creation', GridDataType.DateTime],
        'reference'
      ]}
      loadData={() =>
        app.serviceApi.get<AccountLineView>(`AccountLine/Read/${id}`)
      }
      actions={(data, refresh) => (
        <React.Fragment>
          {hasEditPermisson(data) && (
            <ButtonLink
              variant="outlined"
              href={`/home/account/lines/add/${data.id}`}
              startIcon={<EditIcon />}
            >
              {labels.edit}
            </ButtonLink>
          )}
          {hasApprovalPermission(data) && (
            <Button
              variant="outlined"
              startIcon={<ApprovalIcon />}
              onClick={() => AccountDialogs.Approve(data, true, refresh)}
            >
              {labels.leaderApproval}
            </Button>
          )}
          {hasWriteoffPermission(data) && (
            <Button
              variant="outlined"
              startIcon={<CheckIcon />}
              onClick={() => AccountDialogs.Approve(data, false, refresh)}
            >
              {labels.writeOff}
            </Button>
          )}
        </React.Fragment>
      )}
    >
      {(data) => (
        <AuditDisplay
          loadData={async (rq) => {
            return await app.serviceApi.post<AuditLine[]>(
              'AccountLine/QueryAudit',
              { accountLineId: data.id, ...rq },
              {
                defaultValue: [],
                showLoading: false
              }
            );
          }}
        />
      )}
    </ViewPage>
  );
}

export default ViewLine;
