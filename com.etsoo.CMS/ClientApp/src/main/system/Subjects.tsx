import {
  CommonPage,
  DnDList,
  MoreFab,
  TabBox,
  TabBoxPanel
} from '@etsoo/react';
import React from 'react';
import { app } from '../../app/MyApp';
import ShoppingBasketIcon from '@mui/icons-material/ShoppingBasket';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import AddIcon from '@mui/icons-material/Add';
import RecommendIcon from '@mui/icons-material/Recommend';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import HistoryIcon from '@mui/icons-material/History';
import BarChartIcon from '@mui/icons-material/BarChart';
import {
  Badge,
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
  IconButton,
  Typography,
  useTheme
} from '@mui/material';
import { IActionResult } from '@etsoo/appscript';
import { Subject } from '../../dto/Subject';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { DomUtils } from '@etsoo/shared';

interface ISubjectEdit {
  id?: number;
  deletable?: boolean;
  name: string;
  edited?: boolean;
}

function Subjects() {
  // Route
  const navigate = useNavigate();
  const [search] = useSearchParams();

  // Queries
  const params = DomUtils.dataAs(search, { kind: 'string' });

  // Kind
  const kind = params.kind;

  // Labels
  const labels = app.getLabels(
    'add',
    'delete',
    'expense',
    'income',
    'newSubject',
    'recommendation',
    'save',
    'edit',
    'itemExists',
    'operationSucceeded',
    'subjectHistory',
    'sortTip',
    'reports'
  );

  // Theme
  const theme = useTheme();

  // Save button state
  const [saveEnabled, setSaveEnabled] = React.useState<boolean>(false);

  // Current items
  const itemsRef = React.useRef<{ expense?: Subject[]; income?: Subject[] }>(
    {}
  );

  // Create panel
  const createTab = (
    kind: string,
    label: string,
    icon: React.ReactElement
  ): TabBoxPanel => {
    // Return item
    return {
      label,
      icon,
      to: `?kind=${kind}`,
      wrapped: true,
      children: (
        <Card sx={{ marginTop: 1, marginBottom: 1 }}>
          <DnDList<ISubjectEdit>
            Component={CardContent}
            getListStyle={(_isDraggingOver) => ({
              minHeight: `calc(100vh - ${app.smDown ? '341px' : '357px'})`
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
            labelField="name"
            loadData={async (name) => {
              // Submit
              const result = await app.serviceApi.get<Subject[]>(
                'System/QuerySubjects/' +
                  (name === 'expense' ? 'false' : 'true')
              );
              if (result == null) return [];

              if (kind === 'expense') itemsRef.current.expense = result;
              else itemsRef.current.income = result;

              return result;
            }}
            name={kind}
            onChange={(items) => {
              if (kind === 'expense') itemsRef.current.expense = items;
              else itemsRef.current.income = items;

              const expenseCount = itemsRef.current.expense?.length ?? 0;
              const incomeCount = itemsRef.current.income?.length ?? 0;

              // Limit to 30 maximum
              setSaveEnabled(
                (expenseCount > 0 && expenseCount <= 30) ||
                  (incomeCount > 0 && incomeCount <= 30)
              );
            }}
            sideRenderer={(top, addItem, addItems) => {
              if (top)
                return (
                  <Typography
                    variant="caption"
                    display="block"
                    sx={{ paddingLeft: 2, paddingTop: 2, paddingRight: 2 }}
                  >
                    * {labels.sortTip}
                  </Typography>
                );

              return (
                <CardActions>
                  <Button
                    color="primary"
                    variant="outlined"
                    onClick={() => {
                      app.notifier.prompt(
                        labels.newSubject +
                          ` (${
                            kind === 'expense' ? labels.expense : labels.income
                          })`,
                        (name) => {
                          if (name == null) return;
                          const addResult = addItem({ name, edited: true });
                          if (!addResult) return labels.itemExists.format(name);
                          return addResult;
                        }
                      );
                    }}
                    startIcon={<AddIcon />}
                  >
                    {labels.add}
                  </Button>
                  <Button
                    color="primary"
                    variant="contained"
                    startIcon={<RecommendIcon />}
                    onClick={() => {
                      const items = app.get<string[]>(`${kind}Recommendation`);
                      if (items == null) return;
                      addItems(
                        items.map((item) => ({ name: item, edited: true }))
                      );
                    }}
                  >
                    {labels.recommendation}
                  </Button>
                </CardActions>
              );
            }}
          >
            {(item, index, deleteItem, editItem) => (
              <Grid container spacing={1}>
                <Grid item xs={7}>
                  {item.edited ? (
                    <Badge color="secondary" variant="dot">
                      {item.name}
                    </Badge>
                  ) : (
                    <div>{item.name}</div>
                  )}
                </Grid>
                <Grid
                  item
                  xs={5}
                  display="flex"
                  justifyContent="flex-end"
                  alignItems="center"
                >
                  {item.id && (
                    <IconButton
                      size="small"
                      title={labels.subjectHistory}
                      onClick={() => {
                        navigate(
                          app.transformUrl(
                            `/home/system/subjects/history/${item.id}`
                          )
                        );
                      }}
                    >
                      <HistoryIcon />
                    </IconButton>
                  )}
                  <MoreFab
                    iconButton
                    size="small"
                    anchorOrigin={{
                      vertical: 'bottom',
                      horizontal: 'right'
                    }}
                    transformOrigin={{
                      vertical: 'top',
                      horizontal: 'right'
                    }}
                    actions={[
                      {
                        label: labels.edit,
                        icon: <EditIcon />,
                        action: () => {
                          app.notifier.prompt(
                            labels.newSubject,
                            (name) => {
                              if (name == null || name === item.name)
                                return false;
                              const editResult = editItem(
                                { name, edited: true },
                                index
                              );
                              if (!editResult)
                                return labels.itemExists.format(name);
                              return editResult;
                            },
                            undefined,
                            { defaultValue: item.name }
                          );
                        }
                      },
                      {
                        label: labels.reports,
                        icon: <BarChartIcon />,
                        action: `/home/system/subjects/report/${item.id}`
                      },
                      item.deletable !== false && {
                        label: '-'
                      },
                      item.deletable !== false && {
                        label: labels.delete,
                        icon: <DeleteIcon />,
                        action: () => deleteItem(index)
                      }
                    ]}
                  />
                </Grid>
              </Grid>
            )}
          </DnDList>
        </Card>
      )
    };
  };

  const saveData = async () => {
    // Request data
    const rq = {
      items: [
        ...(itemsRef.current.expense?.map((item, index) => ({
          income: false,
          id: item.id,
          name: item.name,
          orderIndex: index
        })) ?? []),
        ...(itemsRef.current.income?.map((item, index) => ({
          income: true,
          id: item.id,
          name: item.name,
          orderIndex: index
        })) ?? [])
      ]
    };
    if (rq.items.length === 0) return;

    // Submit
    const result = await app.serviceApi.put<IActionResult>(
      'System/UpdateSubjects',
      rq
    );
    if (result == null) return;

    if (result.ok) {
      app.notifier.succeed(
        labels.operationSucceeded + (result.detail ? `(${result.detail})` : ''),
        undefined,
        () => {
          navigate(app.transformUrl('/home/'));
        }
      );
      return;
    }

    app.alertResult(result);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('subjects');
  }, []);

  return (
    <CommonPage>
      <TabBox
        tabs={[
          createTab('expense', labels.expense, <ShoppingBasketIcon />),
          createTab('income', labels.income, <AttachMoneyIcon />)
        ]}
        defaultIndex={kind === 'income' ? 1 : 0}
      />
      <Button
        variant="contained"
        type="submit"
        fullWidth
        disabled={!saveEnabled}
        onClick={saveData}
        startIcon={<SaveIcon />}
      >
        {labels.save}
      </Button>
    </CommonPage>
  );
}

export default Subjects;
