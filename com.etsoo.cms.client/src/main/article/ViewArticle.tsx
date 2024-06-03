import {
  ButtonLink,
  CustomFieldViewer,
  DnDItemStyle,
  ViewPage
} from "@etsoo/materialui";
import { GridDataType, useParamsEx } from "@etsoo/react";
import { DateUtils } from "@etsoo/shared";
import {
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
  Paper,
  Typography,
  useTheme
} from "@mui/material";
import React from "react";
import { app } from "../../app/MyApp";
import {
  ArticleViewDto,
  ArticleViewHistoryDto
} from "../../api/dto/article/ArticleViewDto";
import EditIcon from "@mui/icons-material/Edit";
import CollectionsIcon from "@mui/icons-material/Collections";
import PhotoIcon from "@mui/icons-material/Photo";
import MoreIcon from "@mui/icons-material/More";
import { LocalUtils } from "../../app/LocalUtils";
import { useNavigate } from "react-router-dom";
import { CustomFieldData } from "@etsoo/appscript";

function ViewArticle() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Route
  const navigate = useNavigate();

  // Theme
  const theme = useTheme();

  // Labels
  const labels = app.getLabels(
    "tab",
    "edit",
    "articleView",
    "slideshowLogo",
    "articleLogo",
    "audits",
    "more"
  );

  // View data
  const [audits, setAudits] = React.useState<ArticleViewHistoryDto[]>();
  const [customFields, setCustomField] = React.useState<CustomFieldData[]>([]);

  React.useEffect(() => {
    // Page title
    app.setPageKey("articleView");
  }, []);

  return (
    <ViewPage<ArticleViewDto>
      fields={[
        { data: "title", label: "articleTitle", singleRow: true },
        { data: "subtitle", label: "articleSubtitle", singleRow: true },
        { data: "tabName1", label: labels.tab, singleRow: "medium" },
        { data: "tabName2", label: labels.tab + " 2", singleRow: "medium" },
        { data: "tabName3", label: labels.tab + " 3", singleRow: "medium" },
        { data: "url", label: "articleUrl", singleRow: "medium" },
        { data: "description", label: "articleDescription", singleRow: true },
        { data: "keywords", label: "articleKeywords", singleRow: true },
        { data: "author", label: "user" },
        ["creation", GridDataType.DateTime],
        {
          data: (item) =>
            DateUtils.sameDay(item.creation, item.release) ? (
              app.formatDate(item.release)
            ) : (
              <Typography color="red" variant="subtitle2">
                {app.formatDate(item.release)}
              </Typography>
            ),
          label: "articleRelease"
        },
        {
          data: (item) => (item.weight > 0 ? item.weight : undefined),
          label: "articleWeight"
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        }
      ]}
      loadData={async () => {
        const result = await app.articleApi.viewRead(id);
        setAudits(result?.audits);

        if (result?.data.jsonData) {
          app.websiteApi.queryArticleJsonDataSchema().then((schema) => {
            if (schema == null) return;
            setCustomField(JSON.parse(schema) as CustomFieldData[]);
          });
        }

        return result?.data;
      }}
      actions={(data, refresh) => {
        const logoState = LocalUtils.createLogoState(data);
        return (
          <React.Fragment>
            <ButtonLink
              variant="outlined"
              href={`./../../logo/${data.id}`}
              startIcon={<PhotoIcon />}
              state={logoState}
            >
              {labels.articleLogo}
            </ButtonLink>
            <ButtonLink
              variant="outlined"
              href={`./../../gallery/${data.id}`}
              startIcon={<CollectionsIcon />}
              state={logoState}
            >
              {labels.slideshowLogo}
            </ButtonLink>
            <ButtonLink
              variant="outlined"
              href={`./../../edit/${data.id}`}
              startIcon={<EditIcon />}
            >
              {labels.edit}
            </ButtonLink>
          </React.Fragment>
        );
      }}
    >
      {(data) => (
        <React.Fragment>
          <iframe
            src={app.formatLink(data)}
            width="100%"
            height="480px"
            title={labels.articleView}
          />
          {customFields.length > 0 && data.jsonData != null && (
            <Paper sx={{ padding: 3 }}>
              <CustomFieldViewer
                fields={customFields}
                jsonData={data.jsonData}
              />
            </Paper>
          )}
          <Card sx={{ marginTop: 2 }}>
            <CardActions
              sx={{
                justifyContent: "space-between",
                paddingLeft: 2,
                paddingRight: 2
              }}
            >
              <Typography paddingLeft={1} display="block">
                {labels.audits}
              </Typography>
              <Button
                startIcon={<MoreIcon />}
                onClick={() => navigate(`./../../history/${data.id}`)}
              >
                {labels.more}
              </Button>
            </CardActions>
            <CardContent sx={{ paddingTop: 0 }}>
              {audits?.map((audit, index) => (
                <Grid
                  container
                  item
                  key={audit.rowid}
                  spacing={0}
                  style={DnDItemStyle(index, false, theme)}
                  alignItems="center"
                >
                  <Grid item xs={12} sm={9}>
                    {audit.title}
                  </Grid>
                  <Grid item xs={12} sm={3} textAlign="right">
                    {audit.author}, {app.formatDate(audit.creation)}
                  </Grid>
                </Grid>
              ))}
            </CardContent>
          </Card>
        </React.Fragment>
      )}
    </ViewPage>
  );
}

export default ViewArticle;
