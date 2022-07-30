import {
  ButtonLink,
  GridCellRendererProps,
  GridDataType,
  IconButtonLink,
  MobileListItemRenderer,
  MUGlobal,
  ResponsivePage,
  ScrollerListForwardRef,
  SearchField,
  SelectBool,
  SelectEx,
  Tiplist
} from '@etsoo/react';
import { BoxProps, CardActions, Fab, Typography } from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import { AccountLine } from '../../dto/AccountLine';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import PageviewIcon from '@mui/icons-material/Pageview';
import { EntityStatus, IdLabelDto, UserRole } from '@etsoo/appscript';
import { DateUtils } from '@etsoo/shared';
import { Subject } from '../../dto/Subject';
import { useNavigate, useSearchParams } from 'react-router-dom';

function AccountLines() {
  // Route
  const navigate = useNavigate();
  const [search] = useSearchParams();

  // Labels
  const labels = app.getLabels(
    'add',
    'title',
    'actions',
    'creation',
    'happenDate',
    'amount',
    'subject',
    'edit',
    'status',
    'view',
    'applicant',
    'income',
    'expense'
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef>();

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  // Has edit permission
  const hasEditPermisson = (item: AccountLine) => {
    if (item.entityStatus < EntityStatus.Completed) {
      if (
        (item.selfLine && item.entityStatus === EntityStatus.Normal) ||
        app.hasPermission([UserRole.Finance, UserRole.Admin, UserRole.Founder])
      )
        return true;
    }

    return false;
  };

  const margin = MUGlobal.pagePaddings;
  const happenDateEndRef = React.useRef<HTMLInputElement>();

  React.useEffect(() => {
    // Page title
    app.setPageKey('records');

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <ResponsivePage<AccountLine>
      mRef={ref}
      defaultOrderBy="creation"
      pageProps={{
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.add}
            size="medium"
            color="primary"
            onClick={() =>
              navigate!(app.transformUrl('/home/account/lines/add?kind=e'))
            }
          >
            <AddIcon />
          </Fab>
        )
      }}
      quickAction={(data) => navigate!(`/home/account/lines/view/${data.id}`)}
      fieldTemplate={{
        action: 'string',
        isIncome: 'boolean',
        userUid: 'string'
      }}
      fields={[
        <SearchField label={labels.title} name="title" />,
        <SelectBool label={labels.income} name="isIncome" />,
        <SelectEx<Subject>
          label={labels.subject}
          labelField="name"
          name="subjectId"
          search
          loadData={() =>
            app.serviceApi.get<Subject[]>('System/QuerySubjects/')
          }
        />,
        <Tiplist
          label={labels.applicant}
          name="userUid"
          search
          loadData={async (keyword, id) => {
            return await app.api.post<IdLabelDto<string>[]>(
              'Member/List',
              {
                id,
                keyword
              },
              { defaultValue: [], showLoading: false }
            );
          }}
        />,
        <SearchField
          label={labels.happenDate}
          name="happenDateStart"
          type="date"
          onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
            if (happenDateEndRef.current == null) return;
            happenDateEndRef.current.min = DateUtils.formatForInput(
              event.currentTarget.valueAsDate
            );
          }}
          inputProps={{ max: DateUtils.formatForInput() }}
        />,
        <SearchField
          label=""
          name="happenDateEnd"
          type="date"
          inputRef={happenDateEndRef}
          inputProps={{
            max: DateUtils.formatForInput()
          }}
        />
      ]}
      loadData={async (data) => {
        // Default
        data.action = search.get('action') ?? '';

        return await app.serviceApi.post<AccountLine[]>(
          'AccountLine/Query',
          data,
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: 'happenDate',
          type: GridDataType.Date,
          width: 116,
          header: labels.happenDate,
          sortable: true,
          sortAsc: false
        },
        {
          field: 'subject',
          header: labels.subject,
          width: 120,
          sortable: true
        },
        {
          field: 'user',
          header: labels.applicant,
          width: 110,
          sortable: false
        },
        {
          field: 'title',
          header: labels.title,
          sortable: false
        },
        {
          field: 'amount',
          type: GridDataType.Money,
          width: 120,
          header: labels.amount,
          sortable: true,
          renderProps: app.getMoneyFormatProps()
        },
        {
          field: 'entityStatus',
          width: 100,
          header: labels.status,
          sortable: true,
          valueFormatter: ({ data }) => app.getEntityStatusLabel(data)
        },
        {
          field: 'creation',
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: 120,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<AccountLine, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '6px!important',
              paddingBottom: '6px!important'
            };

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.edit}
                  href={`/home/account/lines/add/${data.id}`}
                  disabled={!hasEditPermisson(data)}
                >
                  <EditIcon />
                </IconButtonLink>
                <IconButtonLink
                  title={labels.view}
                  href={`/home/account/lines/view/${data.id}`}
                >
                  <PageviewIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[140, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.user +
              ', ' +
              data.subject +
              ', ' +
              app.formatMoney(data.amount),
            app.formatDate(data.creation, 'd') +
              ', ' +
              app.getEntityStatusLabel(data),
            [
              hasEditPermisson(data) && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `/home/account/lines/add/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography noWrap>{data.title}</Typography>
            </React.Fragment>,
            <CardActions sx={{ justifyContent: 'flex-end' }}>
              <ButtonLink
                href={`/home/account/lines/view/${data.id}`}
                size="large"
                startIcon={<PageviewIcon />}
              >
                {labels.view}
              </ButtonLink>
            </CardActions>
          ];
        })
      }
    />
  );
}

export default AccountLines;
