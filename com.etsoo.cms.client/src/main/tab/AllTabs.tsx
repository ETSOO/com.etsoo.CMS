import { UserRole } from "@etsoo/appscript";
import {
  CommonPage,
  DnDItemStyle,
  DnDList,
  DnDListRef,
  HBox,
  IconButtonLink,
  SelectEx
} from "@etsoo/materialui";
import {
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
  IconButton,
  Stack,
  Typography,
  useTheme
} from "@mui/material";
import React from "react";
import { useNavigate } from "react-router-dom";
import { app } from "../../app/MyApp";
import { TabDto } from "../../api/dto/tab/TabDto";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import PhotoIcon from "@mui/icons-material/Photo";
import DragIndicatorIcon from "@mui/icons-material/DragIndicator";
import SyncAltIcon from "@mui/icons-material/SyncAlt";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { TabQueryRQ } from "../../api/rq/tab/TabQueryRQ";
import { Utils } from "@etsoo/shared";
import { useSearchParamsEx } from "@etsoo/react";
import { LocalUtils } from "../../app/LocalUtils";

function AllTabs() {
  // Route
  const navigate = useNavigate();

  const defaultParent = useSearchParamsEx({ parent: "number" }).parent;

  // State
  const [parent, setParent] = React.useState<number>();
  const [tabs, setTabs] = React.useState<TabDto[]>([]);
  const [items, setItems] = React.useState<TabDto[]>([]);
  const [currentTab, setTab] = React.useState<TabDto>();

  const tabsRef = React.useRef<TabDto[]>();
  const parentRef = React.useRef<number | undefined>(0);

  // Labels
  const labels = app.getLabels(
    "add",
    "sortTip",
    "parentTab",
    "dragIndicator",
    "edit",
    "delete",
    "tabLogo",
    "sortTipTab",
    "confirmAction",
    "regenerateLink"
  );

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Methods
  const dndRef = React.createRef<DnDListRef<TabDto>>();

  // Theme
  const theme = useTheme();

  // Is mounted or not
  const isMounted = React.useRef(true);

  const updateTabs = (data: TabDto[]) => {
    setItems(data);

    if (data.length > 0) {
      if (tabsRef.current == null) {
        const tabs = [...data];
        Utils.addBlankItem(tabs, "id", "name");
        setTabs(tabs);
        tabsRef.current = tabs;
        return;
      } else if (tabsRef.current.some((tab) => tab.id === data[0].id)) return;

      const index = tabsRef.current.findIndex(
        (tab) => tab.id === data[0].parent
      );

      if (index !== -1) {
        // Update level
        data.forEach(
          (item) => (item.level = (tabsRef.current![index].level ?? 0) + 1)
        );

        tabsRef.current.splice(index + 1, 0, ...data);
        setTabs([...tabsRef.current]);
      }
    }
  };

  const queryTabs = React.useCallback(async (parent?: number) => {
    const rq: TabQueryRQ = {
      queryPaging: { currentPage: 0, batchSize: 100 },
      parent
    };
    const data = await app.tabApi.query(rq);
    if (data == null || !isMounted.current) return;
    updateTabs(data);
  }, []);

  React.useEffect(() => {
    if (defaultParent != null) setParent(defaultParent);
  }, [defaultParent]);

  React.useEffect(() => {
    // Page title
    app.setPageKey("tabs");

    return () => {
      isMounted.current = false;
    };
  }, []);

  React.useEffect(() => {
    // Add tabs as dependency to check updates
    if (parentRef.current === parent) return;
    parentRef.current = parent;
    if (parent != null) {
      if (tabs.some((tab) => tab.id === parent)) {
        // Only one level
        queryTabs(parent);
      } else {
        // Multiple levels
        app.tabApi.ancestorRead(parent).then((data) => {
          if (data == null || data.length === 0 || !isMounted.current) return;
          data.reverse();
          Promise.all(
            data.map((d) =>
              app.tabApi.query(
                { queryPaging: { currentPage: 0, batchSize: 100 }, parent: d },
                { showLoading: false }
              )
            )
          ).then((items) => {
            if (!isMounted.current) return;
            items.forEach((data) => {
              if (data == null || data.length === 0) return;
              updateTabs(data);
            });
          });
        });
      }
    } else {
      queryTabs();
    }
  }, [parent, queryTabs, tabs]);

  return (
    <CommonPage>
      <HBox gap={1}>
        <Stack flexGrow={2}>
          <SelectEx<TabDto>
            label={labels.parentTab}
            labelField="name"
            name="parent"
            search
            fullWidth
            options={tabs}
            onChange={(event) => {
              const p = event.target.value;
              const parent = p === "" ? undefined : (p as number);
              setParent(parent);
            }}
            onItemChange={(item) => setTab(item)}
            itemStyle={(option) => {
              if (option.level == null || option.level === 0) return {};
              return { paddingLeft: `${option.level * 30}px` };
            }}
            value={parent}
          />
        </Stack>
        {currentTab &&
          (currentTab.parent != null ||
            (currentTab.parent == null && currentTab.url != null)) && (
            <IconButton
              onClick={() => {
                setParent(currentTab.parent);
              }}
            >
              <ArrowBackIcon />
            </IconButton>
          )}
      </HBox>
      <Card sx={{ marginTop: 1 }}>
        {adminPermission ? (
          <Typography
            variant="caption"
            display="block"
            sx={{ paddingLeft: 2, paddingTop: 2, paddingRight: 2 }}
          >
            * {labels.sortTipTab}
          </Typography>
        ) : undefined}
        <CardContent>
          <Grid container spacing={0}>
            {adminPermission ? (
              <DnDList<TabDto>
                items={items}
                keyField="id"
                labelField="name"
                onDragEnd={(items) =>
                  app.tabApi.sort(items, { showLoading: false })
                }
                itemRenderer={(item, _index, nodeRef, actionNodeRef) => (
                  <Grid container item spacing={0} {...nodeRef}>
                    <Grid
                      item
                      xs={7}
                      display="flex"
                      justifyContent="flex-start"
                      alignItems="center"
                    >
                      <IconButton
                        style={{ cursor: "move" }}
                        size="small"
                        title={labels.dragIndicator}
                        {...actionNodeRef}
                      >
                        <DragIndicatorIcon />
                      </IconButton>
                      <Button
                        onClick={() => {
                          setParent(item.id);
                        }}
                      >
                        {item.name}
                      </Button>
                    </Grid>
                    <Grid
                      item
                      xs={5}
                      display="flex"
                      justifyContent="flex-end"
                      alignItems="center"
                    >
                      <IconButtonLink
                        title={labels.tabLogo}
                        href={`./../logo/${item.id}`}
                        disabled={!adminPermission}
                        state={LocalUtils.createTabLogoState(item)}
                        size="small"
                      >
                        <PhotoIcon
                          color={item.logo ? "secondary" : undefined}
                        />
                      </IconButtonLink>
                      <IconButtonLink
                        title={labels.edit}
                        href={`./../edit/${item.id}`}
                        disabled={!adminPermission}
                        size="small"
                      >
                        <EditIcon />
                      </IconButtonLink>
                    </Grid>
                  </Grid>
                )}
                mRef={dndRef}
              ></DnDList>
            ) : (
              items.map((item, index) => (
                <Grid
                  container
                  item
                  spacing={0}
                  style={DnDItemStyle(index, false, theme)}
                >
                  <Grid item xs={12} sm={9}></Grid>
                  <Grid
                    item
                    xs={12}
                    sm={3}
                    display="flex"
                    justifyContent="flex-end"
                    alignItems="center"
                  ></Grid>
                </Grid>
              ))
            )}
          </Grid>
        </CardContent>
        {adminPermission ? (
          <CardActions
            sx={{
              justifyContent: "space-between",
              paddingLeft: 2,
              paddingRight: 2
            }}
          >
            <Button
              color="primary"
              variant="outlined"
              onClick={() => navigate(`./../add/?parent=${parent ?? ""}`)}
              startIcon={<AddIcon />}
            >
              {labels.add}
            </Button>
            <Button
              variant="outlined"
              startIcon={<SyncAltIcon />}
              onClick={() => {
                app.notifier.confirm(
                  labels.confirmAction.format(labels.regenerateLink),
                  undefined,
                  async (confirmed) => {
                    if (!confirmed) return;
                    const result = await app.websiteApi.regenerateTabUrls({
                      showLoading: false
                    });
                    if (result == null) return;
                    if (!result.ok) {
                      app.alertResult(result);
                    } else {
                      app.ok();
                    }
                  }
                );
              }}
            >
              {labels.regenerateLink}
            </Button>
          </CardActions>
        ) : undefined}
      </Card>
    </CommonPage>
  );
}

export default AllTabs;
