import React from 'react';
import { CommonPage, DnDItemStyle, MUGlobal } from '@etsoo/materialui';
import { app } from '../../app/MyApp';
import { useNavigate } from 'react-router-dom';
import { DashboardDto } from '../../dto/DashboardDto';
import {
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
  IconButton,
  TextField,
  Typography,
  useTheme
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import MoreIcon from '@mui/icons-material/More';
import HistoryIcon from '@mui/icons-material/History';
import { DomUtils } from '@etsoo/shared';
import { IActionResult } from '@etsoo/appscript';

function Dashboard() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'addArticle',
    'edit',
    'more',
    'audits',
    'initializeWebsite',
    'websiteDomain',
    'websiteTitle',
    'initializeWebsiteTip'
  );

  // Theme
  const theme = useTheme();

  // State
  const [data, setData] = React.useState<DashboardDto>();
  const author = data?.audits[0]?.author;

  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // Load data
  const reloadData = async () => {
    app.api
      .get<DashboardDto>('Website/Dashboard', undefined, { showLoading: false })
      .then((data) => {
        if (data == null) return;

        const domain = data.site.domain;
        if (!domain) {
          app.showInputDialog({
            title: labels.initializeWebsite,
            message: labels.initializeWebsiteTip,
            cancelButton: false,
            fullScreen: app.smDown,
            inputs: (
              <React.Fragment>
                {' '}
                <TextField
                  autoFocus
                  margin="dense"
                  name="domain"
                  required
                  label={labels.websiteDomain}
                  fullWidth
                  variant="standard"
                  inputProps={{ maxLength: 128 }}
                />
                <TextField
                  name="title"
                  margin="dense"
                  variant="standard"
                  label={labels.websiteTitle}
                  required
                  fullWidth
                  multiline
                  rows={2}
                  inputProps={{ maxLength: 128 }}
                />
              </React.Fragment>
            ),
            callback: async (form) => {
              // Cancelled
              if (form == null) {
                return;
              }

              // Form data
              const data = DomUtils.dataAs(new FormData(form), {
                domain: 'string',
                title: 'string'
              });

              // Validation
              if (data.domain == null) {
                DomUtils.setFocus('domain', form);
                return false;
              }

              if (data.title == null) {
                DomUtils.setFocus('title', form);
                return false;
              }

              // Submit
              const result = await app.api.post<IActionResult>(
                'Website/Initialize',
                data,
                {
                  showLoading: false // default will show the loading bar and cause the dialog closed
                }
              );
              if (result == null) return;

              if (result.ok) {
                await reloadData();
                return;
              }

              app.alertResult(result);
            }
          });
        } else {
          app.domain = domain;
          setData(data);
        }
      });
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('menuHome');
  }, []);

  return (
    <CommonPage onUpdateAll={reloadData} paddings={paddings}>
      <Card>
        <CardActions
          sx={{
            justifyContent: 'space-between',
            paddingLeft: 2,
            paddingRight: 2
          }}
        >
          <Button
            color="primary"
            variant="outlined"
            startIcon={<AddIcon />}
            onClick={() => navigate('./article/add')}
          >
            {labels.addArticle}
          </Button>
          <Button
            startIcon={<MoreIcon />}
            onClick={() => navigate('./article/all')}
          >
            {labels.more}
          </Button>
        </CardActions>
        <CardContent sx={{ paddingTop: 0 }}>
          {data?.articles.map((article, index) => (
            <Grid
              container
              item
              key={article.id}
              spacing={0}
              style={DnDItemStyle(index, false, theme)}
              alignItems="center"
            >
              <Grid item xs={12} sm={9}>
                <IconButton
                  size="small"
                  title={labels.edit}
                  onClick={() => navigate(`./article/edit/${article.id}`)}
                  sx={{ marginRight: 1 }}
                >
                  <EditIcon />
                </IconButton>
                {article.title}
              </Grid>
              <Grid item xs={12} sm={3} textAlign="right">
                {article.author}, {app.formatDate(article.refreshTime)}
              </Grid>
            </Grid>
          ))}
        </CardContent>
      </Card>
      <Card sx={{ marginTop: 2 }}>
        <CardActions
          sx={{
            justifyContent: 'space-between',
            paddingLeft: 2,
            paddingRight: 2
          }}
        >
          <Typography paddingLeft={1} display="block">
            {labels.audits}
          </Typography>
          {author && (
            <Button
              startIcon={<MoreIcon />}
              onClick={() => navigate(`./user/history/${author}`)}
            >
              {labels.more}
            </Button>
          )}
        </CardActions>
        <CardContent sx={{ paddingTop: 0 }}>
          {data?.audits.map((audit, index) => (
            <Grid
              container
              item
              key={audit.rowid}
              spacing={0}
              style={DnDItemStyle(index, false, theme)}
              alignItems="center"
            >
              <Grid item xs={12} sm={9}>
                <IconButton size="small" sx={{ marginRight: 1 }}>
                  <HistoryIcon />
                </IconButton>
                {audit.title}
              </Grid>
              <Grid item xs={12} sm={3} textAlign="right">
                {audit.author}, {app.formatDate(audit.creation)}
              </Grid>
            </Grid>
          ))}
        </CardContent>
      </Card>
    </CommonPage>
  );
}

export default Dashboard;
