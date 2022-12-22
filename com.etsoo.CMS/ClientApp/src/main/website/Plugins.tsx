import { UserRole } from '@etsoo/appscript';
import { CommonPage, MUGlobal } from '@etsoo/materialui';
import { Grid } from '@mui/material';
import React from 'react';
import { PluginDto } from '../../api/dto/website/PluginDto';
import { app } from '../../app/MyApp';
import { GAPlugin } from '../../components/GAPlugin';
import { NextJsPlugin } from '../../components/NextJsPlugin';
import { ReCAPTCHAPlugin } from '../../components/ReCAPTCHAPlugin';
import { WXPlugin } from '../../components/WXPlugin';

function Plugins() {
  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // State
  const [items, setItems] = React.useState<PluginDto[]>();

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Load data
  const reloadData = async () => {
    const data = await app.websiteApi.queryServices();
    if (data == null) return;
    setItems(data);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('plugins');
  }, []);

  return (
    <CommonPage onUpdateAll={reloadData} paddings={paddings}>
      <Grid container justifyContent="left" spacing={paddings}>
        <WXPlugin initData={items} disabled={!adminPermission} />
        <GAPlugin initData={items} disabled={!adminPermission} />
        <ReCAPTCHAPlugin initData={items} disabled={!adminPermission} />
        <NextJsPlugin initData={items} disabled={!adminPermission} />
      </Grid>
    </CommonPage>
  );
}

export default Plugins;
