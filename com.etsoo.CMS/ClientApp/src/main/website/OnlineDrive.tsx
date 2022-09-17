import { CommonPage, MUGlobal } from '@etsoo/materialui';
import { Card, CardContent, Typography } from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';

function OnlineDrive() {
  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // Labels
  const labels = app.getLabels('onlineDriveTip');

  // Load data
  const reloadData = async () => {};

  React.useEffect(() => {
    // Page title
    app.setPageKey('onlineDrive');
  }, []);

  return (
    <CommonPage onUpdateAll={reloadData} paddings={paddings}>
      <Typography
        variant="caption"
        display="block"
        sx={{ paddingLeft: 2, paddingRight: 2, paddingBottom: 1 }}
      >
        * {labels.onlineDriveTip}
      </Typography>
      <Card>
        <CardContent>Under construction...</CardContent>
      </Card>
    </CommonPage>
  );
}

export default OnlineDrive;
