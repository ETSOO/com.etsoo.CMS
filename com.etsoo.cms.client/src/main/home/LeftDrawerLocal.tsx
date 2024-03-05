import { ListItemButton, ListItemIcon, ListItemText } from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import ArticleIcon from '@mui/icons-material/Article';
import TabIcon from '@mui/icons-material/Tab';
import ExtensionIcon from '@mui/icons-material/Extension';
import SettingsIcon from '@mui/icons-material/Settings';
import GroupIcon from '@mui/icons-material/Group';
import ListAltIcon from '@mui/icons-material/ListAlt';
import React from 'react';
import { UserRole } from '@etsoo/appscript';
import { app } from '../../app/MyApp';
import { useLocation } from 'react-router-dom';
import { LeftDrawer, LeftDrawerProps, MUGlobal } from '@etsoo/materialui';

export function LeftDrawerLocal(props: LeftDrawerProps) {
  // Labels
  const labels = app.getLabels(
    'etsoo',
    'menuHome',
    'appName',
    'articles',
    'tabs',
    'configs',
    'plugins',
    'users',
    'resources'
  );

  // Location
  // Reload when location changes
  const pathname = useLocation().pathname.replace('/home/', './');

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  const getMenuItem = React.useCallback(
    (href: string) => MUGlobal.getMenuItem(pathname, href),
    [pathname]
  );

  return (
    <LeftDrawer {...props}>
      <ListItemButton {...getMenuItem('./')}>
        <ListItemIcon>
          <HomeIcon />
        </ListItemIcon>
        <ListItemText primary={labels.menuHome} />
      </ListItemButton>
      <ListItemButton {...getMenuItem('./article/all')}>
        <ListItemIcon>
          <ArticleIcon />
        </ListItemIcon>
        <ListItemText primary={labels.articles} />
      </ListItemButton>
      <ListItemButton {...getMenuItem('./tab/all')}>
        <ListItemIcon>
          <TabIcon />
        </ListItemIcon>
        <ListItemText primary={labels.tabs} />
      </ListItemButton>
      <ListItemButton {...getMenuItem('./resource/all')}>
        <ListItemIcon>
          <ListAltIcon />
        </ListItemIcon>
        <ListItemText primary={labels.resources} />
      </ListItemButton>
      <ListItemButton {...getMenuItem('./config/all')}>
        <ListItemIcon>
          <SettingsIcon />
        </ListItemIcon>
        <ListItemText primary={labels.configs} />
      </ListItemButton>
      <ListItemButton {...getMenuItem('./plugin/all')}>
        <ListItemIcon>
          <ExtensionIcon />
        </ListItemIcon>
        <ListItemText primary={labels.plugins} />
      </ListItemButton>
      {adminPermission && (
        <ListItemButton {...getMenuItem('./user/all')}>
          <ListItemIcon>
            <GroupIcon />
          </ListItemIcon>
          <ListItemText primary={labels.users} />
        </ListItemButton>
      )}
    </LeftDrawer>
  );
}
