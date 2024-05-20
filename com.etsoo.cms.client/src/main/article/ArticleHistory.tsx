import {
  DialogButton,
  MUGlobal,
  SearchField,
  ResponsivePage,
  MobileListItemRenderer,
  IsAuditLineUpdateData,
  ShowDataComparison
} from "@etsoo/materialui";
import { DateUtils } from "@etsoo/shared";
import { BoxProps, IconButton, Typography } from "@mui/material";
import InfoIcon from "@mui/icons-material/Info";
import ErrorIcon from "@mui/icons-material/Error";
import WarningIcon from "@mui/icons-material/Warning";
import CompareArrowsIcon from "@mui/icons-material/CompareArrows";
import React from "react";
import { app } from "../../app/MyApp";
import { AuditFlag } from "../../api/dto/user/AuditFlag";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef,
  useParamsEx
} from "@etsoo/react";
import { ArticleHistoryDto } from "../../api/dto/article/ArticleHistoryDto";

function formatData(data: ArticleHistoryDto) {
  const content = data.content;
  let auditData = data.auditData;
  if (typeof content === "string") {
    if (content.startsWith("{")) {
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
      if (key === "auditData") return undefined;
      return value;
    },
    2
  );
}

function ArticleHistory() {
  const { id } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "audits",
    "article",
    "device",
    "successLogin",
    "no",
    "yes",
    "creation",
    "startDate",
    "endDate",
    "language",
    "success",
    "description",
    "actions",
    "title",
    "user",
    "flag",
    "error",
    "warning",
    "type",
    "dataComparison"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<ArticleHistoryDto>>();

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  const margin = MUGlobal.pagePaddings;
  const creationEndRef = React.useRef<HTMLInputElement>();

  React.useEffect(() => {
    // Page title
    app.setPageTitle(labels.audits, labels.article);
  }, []);

  const fieldTemplate = {
    target: "number",
    creationStart: "date",
    creationEnd: "date"
  } as const;

  return (
    <ResponsivePage<ArticleHistoryDto, typeof fieldTemplate>
      mRef={ref}
      defaultOrderBy="creation"
      defaultOrderByAsc={false}
      pageProps={{ onRefresh: reloadData }}
      fieldTemplate={fieldTemplate}
      fields={[
        <input type="hidden" name="target" value={id} />,
        <SearchField
          label={labels.startDate}
          name="creationStart"
          type="date"
          onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
            if (creationEndRef.current == null) return;
            creationEndRef.current.min =
              DateUtils.formatForInput(event.currentTarget.valueAsDate) ?? "";
          }}
          inputProps={{ max: DateUtils.formatForInput(new Date()) }}
        />,
        <SearchField
          label={labels.endDate}
          name="creationEnd"
          type="date"
          inputRef={creationEndRef}
          inputProps={{
            max: DateUtils.formatForInput(new Date())
          }}
        />
      ]}
      loadData={async (data) => {
        return await app.articleApi.history(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "creation",
          type: GridDataType.DateTime,
          width: 164,
          header: labels.creation,
          sortable: true,
          sortAsc: false,
          renderProps: app.getDateFormatProps()
        },
        {
          field: "title",
          header: labels.title,
          sortable: false
        },
        {
          width: 120,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<ArticleHistoryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "9px!important",
              paddingBottom: "9px!important"
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
                        color: "warning",
                        title: labels.warning,
                        icon: <WarningIcon />
                      }
                    : data.flag === AuditFlag.Error
                    ? {
                        color: "error",
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
            app.formatDate(data.creation, "ds"),
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

export default ArticleHistory;
