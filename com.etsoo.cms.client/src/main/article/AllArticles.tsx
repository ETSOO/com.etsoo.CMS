import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  Tiplist,
  VBox,
  InputField
} from "@etsoo/materialui";
import { BoxProps, Fab, IconButton, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import PageviewIcon from "@mui/icons-material/Pageview";
import PublicIcon from "@mui/icons-material/Public";
import PhotoIcon from "@mui/icons-material/Photo";
import SyncAltIcon from "@mui/icons-material/SyncAlt";
import React from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { DataTypes, DomUtils } from "@etsoo/shared";
import { app } from "../../app/MyApp";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { ArticleQueryDto } from "../../api/dto/article/ArticleQueryDto";
import { UserRole } from "@etsoo/appscript";
import { LocalUtils } from "../../app/LocalUtils";

function AllArticles() {
  // Route
  const navigate = useNavigate();
  const location = useLocation();
  const { id } = DomUtils.dataAs(location.state, { id: "string" });

  // Labels
  const labels = app.getLabels(
    "id",
    "creation",
    "actions",
    "add",
    "edit",
    "view",
    "articleTitle",
    "viewWebsite",
    "tab",
    "articleLogo",
    "regenerateLink",
    "link"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<ArticleQueryDto>>();

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey("articles");
  }, []);

  const fieldTemplate = {
    tab: "number",
    title: "string",
    id: "number"
  } as const;

  return (
    <ResponsivePage<ArticleQueryDto, typeof fieldTemplate>
      mRef={ref}
      defaultOrderBy="creation"
      pageProps={{
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <Fab
              title={labels.regenerateLink}
              size="small"
              onClick={() => {
                app.showInputDialog({
                  title: labels.regenerateLink,
                  message: undefined,
                  fullScreen: app.smDown,
                  callback: async (form) => {
                    // Cancelled
                    if (form == null) {
                      return;
                    }

                    // Form data
                    const { url } = DomUtils.dataAs(new FormData(form), {
                      url: "string"
                    });

                    if (!url) {
                      DomUtils.setFocus("url");
                      return false;
                    }

                    // Submit
                    const result = await app.websiteApi.regenerateUrls(
                      url.split(/\s*[;\n]\s*/g),
                      {
                        showLoading: false
                      }
                    );
                    if (result == null) return;

                    if (result.ok) {
                      return;
                    }

                    return app.formatResult(result);
                  },
                  inputs: (
                    <VBox gap={2} marginTop={2}>
                      <InputField
                        fullWidth
                        required
                        name="url"
                        multiline
                        rows={3}
                        inputProps={{ maxLength: 1024 }}
                        label={labels.link}
                      />
                    </VBox>
                  )
                });
              }}
            >
              <SyncAltIcon />
            </Fab>
            <Fab
              title={labels.add}
              size="medium"
              color="primary"
              onClick={() => navigate("./../add")}
            >
              <AddIcon />
            </Fab>
          </React.Fragment>
        )
      }}
      quickAction={(data) => navigate(`./../view/${data.id}`)}
      fields={[
        <SearchField label={labels.articleTitle} name="title" />,
        <Tiplist
          label={labels.tab}
          name="tab"
          search
          loadData={async (keyword, id) => {
            return await app.api.post<DataTypes.IdLabelItem[]>(
              "Tab/List",
              {
                id,
                keyword
              },
              { defaultValue: [], showLoading: false }
            );
          }}
        />,
        <SearchField label={labels.id} name="id" defaultValue={id} />
      ]}
      fieldtemplate={fieldTemplate}
      loadData={(data) =>
        app.articleApi.query(data, { defaultValue: [], showLoading: false })
      }
      columns={[
        {
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          field: "title",
          header: labels.articleTitle,
          sortable: false
        },
        {
          field: "tabName1",
          header: labels.tab,
          width: 240,
          sortable: false
        },
        {
          width: 188,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<ArticleQueryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                {(adminPermission || data.isSelf) && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./../edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {(adminPermission || data.isSelf) && (
                  <IconButtonLink
                    title={labels.articleLogo}
                    href={`./../logo/${data.id}`}
                    state={LocalUtils.createLogoState(data)}
                    size="small"
                  >
                    <PhotoIcon color={data.logo ? "secondary" : undefined} />
                  </IconButtonLink>
                )}
                <IconButtonLink
                  title={labels.view}
                  href={`./../view/${data.id}`}
                >
                  <PageviewIcon />
                </IconButtonLink>
                <IconButton
                  title={labels.viewWebsite}
                  target="_blank"
                  href={app.formatLink(data)}
                >
                  <PublicIcon />
                </IconButton>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[118, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../edit/${data.id}`
              },
              {
                label: labels.articleLogo,
                icon: <PhotoIcon />,
                action: `./../logo/${data.id}`,
                state: LocalUtils.createLogoState(data)
              },
              {
                label: labels.view,
                icon: <PageviewIcon />,
                action: `./../view/${data.id}`
              },
              {
                label: labels.viewWebsite,
                icon: <PublicIcon />,
                action: app.formatLink(data)
              }
            ],
            <React.Fragment>
              <Typography>{data.tabName1}</Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default AllArticles;
