import React from 'react';
import { CommonPage, MUGlobal } from '@etsoo/react';
import { app } from '../../app/MyApp';
import AddIcon from '@mui/icons-material/Add';
import ShoppingBasketIcon from '@mui/icons-material/ShoppingBasket';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import ApprovalIcon from '@mui/icons-material/Approval';
import CheckIcon from '@mui/icons-material/Check';
import { Box, Button, Grid } from '@mui/material';
import { UserRole } from '@etsoo/appscript';
import { useNavigate } from 'react-router-dom';

function Dashboard() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'registerIncome',
    'registerExpense',
    'writeOff',
    'leaderApproval',
    'income',
    'expense'
  );

  // User context
  const Context = app.userState.context;

  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // Load data
  const reloadData = async () => {};

  React.useEffect(() => {
    // Page title
    app.setPageKey('menuHome');
  }, []);

  return (
    <CommonPage onUpdateAll={reloadData} paddings={paddings}>
      <Grid container spacing={1} sx={{ button: { height: 56 } }}>
        <Grid item xs={6} sm={6}>
          <Button
            color="primary"
            variant="outlined"
            fullWidth
            onClick={() =>
              navigate!(app.transformUrl('/home/account/lines/add?kind=e'))
            }
            startIcon={<AddIcon />}
            endIcon={<ShoppingBasketIcon />}
          >
            {labels.registerExpense}
          </Button>
        </Grid>
        <Grid item xs={6} sm={6}>
          <Button
            color="secondary"
            variant="outlined"
            fullWidth
            onClick={() =>
              navigate!(app.transformUrl('/home/account/lines/add?kind=i'))
            }
            startIcon={<AddIcon />}
            endIcon={<AttachMoneyIcon />}
          >
            {labels.registerIncome}
          </Button>
        </Grid>
        <Context.Consumer>
          {() => {
            const user = app.serviceUser;

            const hasPermission = app.hasPermission([
              UserRole.Finance,
              UserRole.Admin,
              UserRole.Founder
            ]);

            return (
              <React.Fragment>
                {user?.leaderApprovalRequired && hasPermission && (
                  <Grid item xs={6} sm={6}>
                    <Button
                      color="primary"
                      variant="outlined"
                      fullWidth
                      onClick={() =>
                        navigate!(
                          app.transformUrl(
                            '/home/account/lines/?action=approval'
                          )
                        )
                      }
                      startIcon={<ApprovalIcon />}
                    >
                      {labels.leaderApproval}
                    </Button>
                  </Grid>
                )}
                {user?.writeOffRequired && hasPermission && (
                  <Grid item xs={6} sm={6}>
                    <Button
                      color="primary"
                      variant="outlined"
                      fullWidth
                      onClick={() =>
                        navigate!(
                          app.transformUrl(
                            '/home/account/lines/?action=confirm'
                          )
                        )
                      }
                      startIcon={<CheckIcon />}
                    >
                      {labels.writeOff}
                    </Button>
                  </Grid>
                )}
              </React.Fragment>
            );
          }}
        </Context.Consumer>
      </Grid>
      <Box paddingTop={paddings}></Box>
    </CommonPage>
  );
}

export default Dashboard;
