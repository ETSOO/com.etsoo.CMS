import { CommonPage, DnDItemStyle, MUGlobal } from '@etsoo/materialui';
import {
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
  IconButton,
  TextField,
  useTheme
} from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import { ResourceDto } from '../../dto/ResourceDto';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import AddToDriveIcon from '@mui/icons-material/AddToDrive';
import { IActionResult, UserRole } from '@etsoo/appscript';
import { DomUtils } from '@etsoo/shared';
import { useNavigate } from 'react-router-dom';

function Resources() {
  // Route
  const navigate = useNavigate();

  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // Labels
  const labels = app.getLabels(
    'add',
    'edit',
    'id',
    'resourceValue',
    'onlineDrive'
  );

  // State
  const [items, setItems] = React.useState<ResourceDto[]>([]);

  // Theme
  const theme = useTheme();

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Load data
  const reloadData = async () => {
    app.api.post<ResourceDto[]>('Website/QueryResources').then((data) => {
      if (data == null) return;
      setItems(data);
    });
  };

  const showModal = (item?: ResourceDto) => {
    app.showInputDialog({
      title: item ? labels.edit : labels.add,
      message: '',
      fullScreen: app.smDown,
      inputs: (
        <React.Fragment>
          {' '}
          <TextField
            autoFocus
            margin="dense"
            name="id"
            required
            label={labels.id}
            fullWidth
            variant="standard"
            defaultValue={item?.id}
            inputProps={{ maxLength: 50 }}
          />
          <TextField
            name="value"
            margin="dense"
            variant="standard"
            label={labels.resourceValue}
            required
            fullWidth
            multiline
            rows={2}
            defaultValue={item?.value}
            inputProps={{ maxLength: 1280 }}
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
          id: 'string',
          value: 'string'
        });

        // Validation
        if (data.id == null) {
          DomUtils.setFocus('id', form);
          return false;
        }

        if (data.value == null) {
          DomUtils.setFocus('value', form);
          return false;
        }

        // Submit
        const result = await app.api.put<IActionResult>(
          'Website/CreateOrUpdateResource',
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
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('resources');
  }, []);

  return (
    <CommonPage onUpdateAll={reloadData} paddings={paddings}>
      <Card>
        {adminPermission ? (
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
              onClick={() =>
                navigate(app.transformUrl('/home/resource/onlinedrive'))
              }
              startIcon={<AddToDriveIcon />}
            >
              {labels.onlineDrive}
            </Button>
            <Button
              color="primary"
              variant="outlined"
              onClick={() => showModal()}
              startIcon={<AddIcon />}
            >
              {labels.add}
            </Button>
          </CardActions>
        ) : undefined}
        <CardContent sx={{ paddingTop: 0 }}>
          {items.map((item, index) => (
            <Grid
              container
              item
              key={item.id}
              spacing={0}
              style={DnDItemStyle(index, false, theme)}
              alignItems="center"
            >
              <Grid item xs={6} sm={3}>
                <IconButton
                  size="small"
                  title={labels.edit}
                  onClick={() => showModal(item)}
                  sx={{ marginRight: 1 }}
                >
                  <EditIcon />
                </IconButton>
                {item.id}
              </Grid>
              <Grid item xs={6} sm={9}>
                {item.value}
              </Grid>
            </Grid>
          ))}
        </CardContent>
      </Card>
    </CommonPage>
  );
}

export default Resources;
