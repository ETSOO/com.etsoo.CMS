import { EntityStatus } from '@etsoo/appscript';
import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  ComboBox,
  IconButtonLink,
  MobileListItemRenderer
} from '@etsoo/materialui';
import { Box, BoxProps, Fab, IconButton, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import LockResetIcon from '@mui/icons-material/LockReset';
import BlockIcon from '@mui/icons-material/Block';
import HistoryIcon from '@mui/icons-material/History';
import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { DomUtils } from '@etsoo/shared';
import { app } from '../../app/MyApp';
import { UserDto } from '../../dto/UserDto';
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from '@etsoo/react';

function AllUsers() {
  // Route
  const navigate = useNavigate();
  const location = useLocation();
  const { id } = DomUtils.dataAs(location.state, { id: 'string' });

  // Roles
  const roles = app.getLocalRoles();

  const getRoleLabel = (data?: UserDto) => {
    var role = data?.role;
    if (role == null) return '';
    return app
      .getRoles(role)
      .map((r) => r.label)
      .join(', ');
  };

  // Labels
  const labels = app.getLabels(
    'id',
    'name',
    'organization',
    'actions',
    'role',
    'add',
    'edit',
    'inactivated',
    'entityStatus',
    'confirmAction',
    'lastActive',
    'resetPassword',
    'confirmAction',
    'audits'
  );

  const resetPassword = (id: string) => {
    app.notifier.confirm(
      labels.confirmAction.format(labels.resetPassword),
      undefined,
      async (confirmed) => {
        if (!confirmed) return;
        app.resetPassword(id);
      }
    );
  };

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<UserDto>>();

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey('users');
  }, []);

  return (
    <ResponsivePage<UserDto>
      mRef={ref}
      defaultOrderBy="creation"
      pageProps={{
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <Fab
              title={labels.add}
              size="medium"
              color="primary"
              onClick={() => navigate(app.transformUrl('/home/user/add'))}
            >
              <AddIcon />
            </Fab>
          </React.Fragment>
        )
      }}
      fieldTemplate={{ role: 'number' }}
      fields={[
        <SearchField label={labels.id} name="sid" defaultValue={id} />,
        <ComboBox options={roles} name="role" label={labels.role} search />
      ]}
      loadData={async (data) => {
        return await app.api.post<UserDto[]>('User/Query', data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: 'id',
          header: labels.id,
          sortable: true
        },
        {
          field: 'role',
          width: 180,
          header: labels.role,
          valueFormatter: ({ data }) => getRoleLabel(data),
          sortable: false
        },
        {
          field: 'status',
          width: 90,
          header: labels.entityStatus,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<UserDto, BoxProps>) => {
            if (data == null || data.status < EntityStatus.Inactivated)
              return undefined;

            cellProps.sx = {
              paddingTop: '14px!important',
              paddingBottom: '10px!important'
            };

            return (
              <Box title={app.getStatusLabel(data.status)}>
                <BlockIcon color="error" />
              </Box>
            );
          }
        },
        {
          field: 'refreshTime',
          type: GridDataType.Date,
          width: 116,
          header: labels.lastActive,
          sortable: true,
          sortAsc: false
        },
        {
          width: 156,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<UserDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '9px!important',
              paddingBottom: '9px!important'
            };

            return (
              <React.Fragment>
                {!data.isSelf && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`/home/user/edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {!data.isSelf && (
                  <IconButton
                    title={labels.resetPassword}
                    onClick={() => resetPassword(data.id)}
                  >
                    <LockResetIcon />
                  </IconButton>
                )}
                <IconButtonLink
                  title={labels.audits}
                  href={`/home/user/history/${data.id}`}
                >
                  <HistoryIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[86, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.id,
            app.formatDate(data.refreshTime, 'd'),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `/home/user/edit/${data.id}`
              },
              {
                label: labels.resetPassword,
                icon: <LockResetIcon />,
                action: () => resetPassword(data.id)
              },
              {
                label: labels.audits,
                icon: <HistoryIcon />,
                action: `/home/user/history/${data.id}`
              }
            ],
            <React.Fragment>
              {data.status >= EntityStatus.Inactivated && (
                <React.Fragment>
                  <Typography variant="caption">
                    {labels.entityStatus + ': '}
                  </Typography>
                  <Typography
                    variant="caption"
                    color={(theme) => theme.palette.error.main}
                  >
                    {app.getStatusLabel(data.status)}
                  </Typography>
                </React.Fragment>
              )}
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default AllUsers;
