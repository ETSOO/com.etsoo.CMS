import { UserRole } from '@etsoo/appscript';
import { CommonPage, MUGlobal } from '@etsoo/materialui';
import { Grid } from '@mui/material';
import React from 'react';
import { app } from '../../app/MyApp';
import { GAPlugin } from '../../components/GAPlugin';
import { WXPlugin } from '../../components/WXPlugin';
import { PluginDto } from '../../dto/PluginDto';

function Plugins() {
  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // State
  const [items, setItems] = React.useState<PluginDto[]>();

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Load data
  const reloadData = async () => {
    app.api.post<PluginDto[]>('Website/QueryServices').then((data) => {
      if (data == null) return;
      setItems(data);
    });
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('plugins');
  }, []);

  return (
    <CommonPage onUpdateAll={reloadData} paddings={paddings}>
      <Grid container justifyContent="left" spacing={paddings}>
        <GAPlugin initData={items} disabled={!adminPermission} />
        <WXPlugin initData={items} disabled={!adminPermission} />
      </Grid>
    </CommonPage>
  );
}

export default Plugins;
