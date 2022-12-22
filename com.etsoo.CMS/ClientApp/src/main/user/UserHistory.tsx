import {
  DialogButton,
  MUGlobal,
  SearchField,
  ResponsivePage,
  MobileListItemRenderer,
  ComboBox,
  IsAuditLineUpdateData,
  ShowDataComparison
} from '@etsoo/materialui';
import { DateUtils } from '@etsoo/shared';
import { BoxProps, IconButton, Typography } from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import CompareArrowsIcon from '@mui/icons-material/CompareArrows';
import React from 'react';
import { app } from '../../app/MyApp';
import { UserHistoryDto } from '../../api/dto/user/UserHistoryDto';
import { useParams } from 'react-router-dom';
import { AuditFlag } from '../../api/dto/user/AuditFlag';
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from '@etsoo/react';

function formatData(data: UserHistoryDto) {
  const content = data.content;
  let auditData = data.auditData;
  if (typeof content === 'string') {
    if (content.startsWith('{')) {
      try {
        const json = JSON.parse(content);
        if (IsAuditLineUpdateData(json)) {
          auditData = json;
          data.auditData = auditData;
          data.content = undefined;
        } else data.content = json;
      } catch (err) {
        console.log(err, content);
      }
    }
  }
  return auditData;
}

function formatJsonData(data: any) {
  return JSON.stringify(
    data,
    (key, value) => {
      if (key === 'auditData') return undefined;
      return value;
    },
    2
  );
}

function UserHistory() {
  const { id } = useParams<{ id: string }>();

  // Labels
  const labels = app.getLabels(
    'device',
    'successLogin',
    'no',
    'yes',
    'creation',
    'startDate',
    'endDate',
    'language',
    'success',
    'description',
    'actions',
    'title',
    'user',
    'flag',
    'error',
    'warning',
    'type',
    'dataComparison'
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<UserHistoryDto>>();

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  const margin = MUGlobal.pagePaddings;
  const creationEndRef = React.useRef<HTMLInputElement>();

  React.useEffect(() => {
    // Page title
    app.setPageKey('audits');
  }, []);

  return (
    <ResponsivePage<UserHistoryDto>
      mRef={ref}
      defaultOrderBy="creation"
      defaultOrderByAsc={false}
      pageProps={{ onRefresh: reloadData }}
      fieldTemplate={{ author: 'string', kind: 'number' }}
      fields={[
        <ComboBox
          options={app.getAuditKinds()}
          name="kind"
          label={labels.type}
          search
        />,
        <SearchField
          label={labels.startDate}
          name="creationStart"
          type="date"
          onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
            if (creationEndRef.current == null) return;
            creationEndRef.current.min = DateUtils.formatForInput(
              event.currentTarget.valueAsDate
            );
          }}
          inputProps={{ max: DateUtils.formatForInput() }}
        />,
        <SearchField
          label={labels.endDate}
          name="creationEnd"
          type="date"
          inputRef={creationEndRef}
          inputProps={{
            max: DateUtils.formatForInput()
          }}
        />
      ]}
      loadData={async (data) => {
        if (!id) return null;
        data.author = id;
        return await app.userApi.history(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: 'creation',
          type: GridDataType.DateTime,
          width: 164,
          header: labels.creation,
          sortable: true,
          sortAsc: false,
          renderProps: app.getDateFormatProps()
        },
        {
          field: 'title',
          header: labels.title,
          sortable: false
        },
        {
          width: 120,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<UserHistoryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '9px!important',
              paddingBottom: '9px!important'
            };

            const auditData = formatData(data);

            return (
              <React.Fragment>
                <DialogButton
                  content={formatJsonData(data)}
                  contentPre
                  disableScrollLock
                  maxWidth="xs"
                  size="small"
                  dialogTitle="Json Data"
                  {...(data.flag === AuditFlag.Warning
                    ? {
                        color: 'warning',
                        title: labels.warning,
                        icon: <WarningIcon />
                      }
                    : data.flag === AuditFlag.Error
                    ? {
                        color: 'error',
                        title: labels.error,
                        icon: <ErrorIcon />
                      }
                    : { color: undefined, icon: <InfoIcon /> })}
                />
                {auditData && (
                  <IconButton
                    title={labels.dataComparison}
                    size="small"
                    color="info"
                    onClick={() => ShowDataComparison(auditData!)}
                  >
                    <CompareArrowsIcon />
                  </IconButton>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[134, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          const auditData = formatData(data);
          return [
            data.title,
            app.formatDate(data.creation, 'ds'),
            <React.Fragment>
              <DialogButton
                content={JSON.stringify(data, undefined, 2)}
                contentPre
                disableScrollLock
                maxWidth="xs"
                size="small"
                icon={<InfoIcon />}
              >
                JSON data
              </DialogButton>
              {auditData && (
                <IconButton
                  title={labels.dataComparison}
                  size="small"
                  color="info"
                  onClick={() => ShowDataComparison(auditData)}
                >
                  <CompareArrowsIcon />
                </IconButton>
              )}
            </React.Fragment>,
            <React.Fragment>
              <Typography variant="caption" noWrap></Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default UserHistory;
