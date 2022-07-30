import {
  CommonPage,
  DataGridRenderers,
  DnDList,
  GridCellRendererProps,
  GridDataType,
  IconButtonLink,
  MobileListItemRenderer,
  MoneyText,
  MUGlobal,
  ResponsibleContainer,
  SearchField,
  Switch,
  TabBox,
  TabBoxPanel,
  TooltipClick
} from '@etsoo/react';
import {
  Box,
  BoxProps,
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
  IconButton,
  Typography,
  useTheme
} from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { Account } from '../../dto/Account';
import { AccountDialogs } from './AccountDialogs';
import BlockIcon from '@mui/icons-material/Block';
import HistoryIcon from '@mui/icons-material/History';
import { EntityStatus, UserRole } from '@etsoo/appscript';
import { useNavigate, useSearchParams } from 'react-router-dom';

function CompanyAccounts() {
  // Route
  const navigate = useNavigate();

  // Queries
  const [search] = useSearchParams();

  // Kind
  const kind = search.get('kind');

  // Labels
  const labels = app.getLabels(
    'companyAccounts',
    'externalAccounts',
    'add',
    'edit',
    'delete',
    'accountBalance',
    'completeTip',
    'copyAsPayment',
    'showIt',
    'addCashAccount',
    'accountBank',
    'accountName',
    'accountNumber',
    'creation',
    'status',
    'statusNormal',
    'actions',
    'accountHistory',
    'sortTip'
  );

  // Permissions
  const userPermission = app.hasPermission([
    UserRole.Finance,
    UserRole.Admin,
    UserRole.Founder
  ]);
  const financePermission = app.hasPermission([
    UserRole.Finance,
    UserRole.Founder
  ]);

  // Theme
  const theme = useTheme();

  const tabs: TabBoxPanel[] = [
    {
      label: labels.companyAccounts,
      to: '?kind=c',
      children: (
        <Card sx={{ marginTop: 1 }}>
          <DnDList<Account>
            Component={CardContent}
            disabled={!userPermission}
            getListStyle={(_isDraggingOver) => ({
              minHeight: `calc(100vh - ${
                app.smDown
                  ? userPermission
                    ? '265px'
                    : '209px'
                  : userPermission
                  ? '289px'
                  : '201px'
              })`
            })}
            getItemStyle={(isDragging, index) => ({
              userSelect: 'none',
              padding: theme.spacing(1),
              background: isDragging
                ? theme.palette.primary.light
                : index % 2 === 0
                ? theme.palette.grey[100]
                : theme.palette.grey[50]
            })}
            labelField="displayName"
            loadData={async () => {
              // Submit
              const result = await app.serviceApi.get<Account[]>(
                'Account/CompanyAccounts/all'
              );
              if (result == null) return [];

              return result;
            }}
            name="companyAccount"
            onDragEnd={(items) => {
              const rq: Record<number, number> = {};
              items.forEach((item, index) => (rq[item.id] = index));
              app.serviceApi.put('Account/SortAccounts', rq, {
                // No indicator for loading
                showLoading: false
              });
            }}
            sideRenderer={(top, _addItem, _addItems, reloadItems) => {
              if (top)
                return userPermission ? (
                  <Typography
                    variant="caption"
                    display="block"
                    sx={{ paddingLeft: 2, paddingTop: 2, paddingRight: 2 }}
                  >
                    * {labels.sortTip}
                  </Typography>
                ) : undefined;

              return userPermission ? (
                <CardActions>
                  <Button
                    color="primary"
                    variant="outlined"
                    onClick={() =>
                      navigate!(
                        app.transformUrl('/home/account/company/add/?kind=c')
                      )
                    }
                    startIcon={<AddIcon />}
                  >
                    {labels.add}
                  </Button>
                  <Button
                    color="primary"
                    variant="contained"
                    onClick={() => {
                      AccountDialogs.createCashAccount(reloadItems);
                    }}
                    startIcon={<AddIcon />}
                  >
                    {labels.addCashAccount}
                  </Button>
                </CardActions>
              ) : undefined;
            }}
          >
            {(item, index, _deleteItem, editItem) => (
              <Grid container spacing={1}>
                <Grid item xs={12} sm={9}>
                  <span
                    style={{
                      textDecoration: item.enabled ? 'inherit' : 'line-through'
                    }}
                  >
                    {item.accountBank}, {item.accountNumber}
                  </span>
                  <br />
                  {labels.accountBalance}:{' '}
                  {item.accountBalance == null ? (
                    '*********'
                  ) : (
                    <MoneyText
                      value={item.accountBalance}
                      currency={app.currency}
                    />
                  )}
                  &nbsp;
                  {financePermission && (
                    <IconButton
                      size="small"
                      title={labels.showIt}
                      onClick={async () => {
                        if (item.accountBalance != null) {
                          editItem(
                            {
                              ...item,
                              accountBalance: undefined
                            },
                            index
                          );
                          return;
                        }

                        const result = await app.serviceApi.get<number>(
                          `Account/GetBalance/${item.id}`
                        );
                        if (result == null) return;
                        editItem(
                          {
                            ...item,
                            accountBalance: result
                          },
                          index
                        );
                      }}
                    >
                      <VisibilityIcon />
                    </IconButton>
                  )}
                </Grid>
                <Grid
                  item
                  xs={12}
                  sm={3}
                  display="flex"
                  justifyContent="flex-end"
                  alignItems="center"
                >
                  <TooltipClick
                    disableHoverListener={false}
                    title={labels.copyAsPayment}
                  >
                    {(openTooltip) => (
                      <IconButton
                        size="small"
                        onClick={async () => {
                          const result = await app.serviceApi.get<string>(
                            `Account/GetPaymentData/${item.id}`
                          );
                          if (result == null) return;

                          navigator.clipboard?.writeText(result);
                          openTooltip(
                            labels.completeTip.format(labels.copyAsPayment)
                          );
                        }}
                      >
                        <ContentCopyIcon />
                      </IconButton>
                    )}
                  </TooltipClick>
                  {userPermission && (
                    <React.Fragment>
                      <IconButton
                        size="small"
                        title={labels.edit}
                        onClick={() => {
                          navigate!(
                            app.transformUrl(
                              `/home/account/company/add/${item.id}`
                            )
                          );
                        }}
                      >
                        <EditIcon />
                      </IconButton>
                      <IconButton
                        size="small"
                        title={labels.accountHistory}
                        onClick={() => {
                          navigate!(
                            app.transformUrl(
                              `/home/account/company/history/${item.id}`
                            )
                          );
                        }}
                      >
                        <HistoryIcon />
                      </IconButton>
                    </React.Fragment>
                  )}
                </Grid>
              </Grid>
            )}
          </DnDList>
        </Card>
      )
    }
  ];

  const margin = MUGlobal.pagePaddings;

  if (userPermission)
    tabs.push({
      label: labels.externalAccounts,
      to: '?kind=e',
      children: (visible: boolean) => (
        <Box sx={{ paddingTop: margin }}>
          {visible ? (
            <ResponsibleContainer<Account>
              defaultOrderBy="creation"
              fieldTemplate={{
                accountName: 'string',
                accountNumber: 'string',
                enabled: 'boolean'
              }}
              fields={[
                <SearchField label={labels.accountName} name="accountName" />,
                <SearchField
                  label={labels.accountNumber}
                  name="accountNumber"
                />,
                <Switch
                  label={labels.statusNormal}
                  name="enabled"
                  defaultChecked
                />
              ]}
              footerItemRenderer={(rows, props) => {
                if (props.index === 0) {
                  return (
                    <Button
                      color="primary"
                      variant="outlined"
                      onClick={() =>
                        navigate!(
                          app.transformUrl('/home/account/company/add/?kind=e')
                        )
                      }
                      startIcon={<AddIcon />}
                    >
                      {labels.add}
                    </Button>
                  );
                }

                return DataGridRenderers.defaultFooterItemRenderer(
                  rows,
                  props,
                  4
                );
              }}
              loadData={async (data) => {
                return await app.serviceApi.post<Account[]>(
                  'Account/Query',
                  data,
                  {
                    defaultValue: [],
                    showLoading: false
                  }
                );
              }}
              columns={[
                {
                  field: 'accountName',
                  header: labels.accountName,
                  sortable: true
                },
                {
                  field: 'accountBank',
                  width: 240,
                  header: labels.accountBank,
                  sortable: false
                },
                {
                  field: 'accountNumber',
                  width: 118,
                  header: labels.accountNumber,
                  sortable: false
                },
                {
                  field: 'enabled',
                  width: 90,
                  header: labels.status,
                  align: 'center',
                  cellRenderer: ({
                    data,
                    cellProps
                  }: GridCellRendererProps<Account, BoxProps>) => {
                    if (
                      data == null ||
                      data.entityStatus < EntityStatus.Inactivated
                    )
                      return undefined;

                    cellProps.sx = {
                      paddingTop: '14px!important',
                      paddingBottom: '10px!important'
                    };

                    return (
                      <Box title={app.getEntityStatusLabel(data)}>
                        <BlockIcon color="warning" />
                      </Box>
                    );
                  },
                  sortable: false
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
                  width: 156,
                  header: labels.actions,
                  cellRenderer: ({
                    data,
                    cellProps
                  }: GridCellRendererProps<Account, BoxProps>) => {
                    if (data == null) return undefined;

                    cellProps.sx = {
                      paddingTop: '6px!important',
                      paddingBottom: '6px!important'
                    };

                    return (
                      <React.Fragment>
                        <IconButtonLink
                          title={labels.edit}
                          href={`/home/account/company/add/${data.id}`}
                        >
                          <EditIcon />
                        </IconButtonLink>
                        <TooltipClick
                          disableHoverListener={false}
                          title={labels.copyAsPayment}
                        >
                          {(openTooltip) => (
                            <IconButton
                              size="small"
                              onClick={async () => {
                                const result = await app.serviceApi.get<string>(
                                  `Account/GetPaymentData/${data.id}`
                                );
                                if (result == null) return;

                                navigator.clipboard?.writeText(result);
                                openTooltip(
                                  labels.completeTip.format(
                                    labels.copyAsPayment
                                  )
                                );
                              }}
                            >
                              <ContentCopyIcon />
                            </IconButton>
                          )}
                        </TooltipClick>
                        <IconButtonLink
                          title={labels.accountHistory}
                          href={`/home/account/company/history/${data.id}`}
                        >
                          <HistoryIcon />
                        </IconButtonLink>
                      </React.Fragment>
                    );
                  }
                }
              ]}
              itemSize={[96, margin, false]}
              innerItemRenderer={(props) =>
                MobileListItemRenderer(props, (data) => {
                  return [
                    data.accountName,
                    app.formatDate(data.creation, 'd'),
                    [
                      {
                        label: labels.edit,
                        icon: <EditIcon />,
                        action: `/home/account/company/add/${data.id}`
                      },
                      {
                        label: labels.copyAsPayment,
                        icon: <ContentCopyIcon />,
                        action: () => {}
                      },
                      {
                        label: labels.accountHistory,
                        icon: <HistoryIcon />,
                        action: `/home/account/company/history/${data.id}`
                      }
                    ],
                    <React.Fragment>
                      <Typography noWrap>
                        {data.accountBank}, {data.accountNumber}
                      </Typography>
                    </React.Fragment>
                  ];
                })
              }
            />
          ) : undefined}
        </Box>
      )
    });

  React.useEffect(() => {
    // Page title
    app.setPageKey('accounts');

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <CommonPage>
      <TabBox tabs={tabs} defaultIndex={kind === 'e' ? 1 : 0} />
    </CommonPage>
  );
}

export default CompanyAccounts;
