import { UserRole } from '@etsoo/appscript';
import {
  CommonPage,
  DnDItemStyle,
  DnDList,
  DnDListRef,
  IconButtonLink,
  SelectEx
} from '@etsoo/materialui';
import {
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
import { useNavigate } from 'react-router-dom';
import { app } from '../../app/MyApp';
import { TabDto } from '../../api/dto/tab/TabDto';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DragIndicatorIcon from '@mui/icons-material/DragIndicator';
import { TabQueryRQ } from '../../api/rq/tab/TabQueryRQ';
import { Utils } from '@etsoo/shared';
import { useSearchParamsEx } from '@etsoo/react';

function AllTabs() {
  // Route
  const navigate = useNavigate();

  const defaultParent = useSearchParamsEx({ parent: 'number' }).parent;

  // State
  const [parent, setParent] = React.useState<number>();
  const [tabs, setTabs] = React.useState<TabDto[]>([]);
  const [items, setItems] = React.useState<TabDto[]>([]);

  const tabsRef = React.useRef<TabDto[]>();

  // Labels
  const labels = app.getLabels(
    'add',
    'sortTip',
    'parentTab',
    'dragIndicator',
    'edit',
    'delete'
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
        Utils.addBlankItem(tabs, 'id', 'name');
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
    const rq: TabQueryRQ = { currentPage: 0, batchSize: 100, parent };
    const data = await app.tabApi.query(rq);
    if (data == null || !isMounted.current) return;
    updateTabs(data);
  }, []);

  React.useEffect(() => {
    if (defaultParent != null) setParent(defaultParent);
  }, [defaultParent]);

  React.useEffect(() => {
    // First level
    queryTabs();
  }, [queryTabs]);

  React.useEffect(() => {
    // Page title
    app.setPageKey('tabs');

    return () => {
      isMounted.current = false;
    };
  }, []);

  React.useEffect(() => {
    // Add tabs as dependency to check updates
    if (defaultParent != null) {
      if (tabs.some((tab) => tab.id === defaultParent)) {
        // Only one level
        queryTabs(defaultParent);
      } else {
        // Multiple levels
        app.tabApi.ancestorRead(defaultParent).then((data) => {
          if (data == null || data.length === 0 || !isMounted.current) return;
          data.reverse();
          Promise.all(
            data.map((d) =>
              app.tabApi.query(
                { currentPage: 0, batchSize: 100, parent: d },
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
    }
  }, [defaultParent, queryTabs, tabs]);

  return (
    <CommonPage>
      <SelectEx<TabDto>
        label={labels.parentTab}
        labelField="name"
        name="parent"
        search
        fullWidth
        options={tabs}
        onChange={(event) => {
          const p = event.target.value;
          const parent = p === '' ? undefined : (p as number);
          setParent(parent);
          queryTabs(parent);
        }}
        itemStyle={(option) => {
          if (option.level == null || option.level === 0) return {};
          return { paddingLeft: `${option.level * 30}px` };
        }}
        value={defaultParent}
      />
      <Card sx={{ marginTop: 1 }}>
        {adminPermission ? (
          <Typography
            variant="caption"
            display="block"
            sx={{ paddingLeft: 2, paddingTop: 2, paddingRight: 2 }}
          >
            * {labels.sortTip}
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
                itemRenderer={(item, index, nodeRef, actionNodeRef) => (
                  <Grid container item spacing={0} {...nodeRef}>
                    <Grid
                      item
                      xs={7}
                      display="flex"
                      justifyContent="flex-start"
                      alignItems="center"
                    >
                      <IconButton
                        style={{ cursor: 'move' }}
                        size="small"
                        title={labels.dragIndicator}
                        {...actionNodeRef}
                      >
                        <DragIndicatorIcon />
                      </IconButton>
                      <Typography>{item.name}</Typography>
                    </Grid>
                    <Grid
                      item
                      xs={5}
                      display="flex"
                      justifyContent="flex-end"
                      alignItems="center"
                    >
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
          <CardActions>
            <Button
              color="primary"
              variant="outlined"
              onClick={() => navigate(`./../add/?parent=${parent ?? ''}`)}
              startIcon={<AddIcon />}
            >
              {labels.add}
            </Button>
          </CardActions>
        ) : undefined}
      </Card>
    </CommonPage>
  );
}

export default AllTabs;
