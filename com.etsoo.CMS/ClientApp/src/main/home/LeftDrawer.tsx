import {
  Avatar,
  Divider,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  styled,
  Typography
} from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import ArticleIcon from '@mui/icons-material/Article';
import TabIcon from '@mui/icons-material/Tab';
import ExtensionIcon from '@mui/icons-material/Extension';
import SettingsIcon from '@mui/icons-material/Settings';
import GroupIcon from '@mui/icons-material/Group';
import ListAltIcon from '@mui/icons-material/ListAlt';
import React from 'react';
import { CSSProperties } from 'react';
import { UserRole } from '@etsoo/appscript';
import { app } from '../../app/MyApp';
import { useLocation } from 'react-router-dom';
import { MUGlobal } from '@etsoo/materialui';

export const DrawerHeader = styled('div')(({ theme }) => ({
  // necessary for content to be below app bar
  ...(theme.mixins.toolbar as CSSProperties),
  display: 'flex',
  alignItems: 'center',
  padding: theme.spacing(0, 2.5, 0, 2.5),
  justifyContent: 'flex-start'
}));

export interface LeftDrawerMethods {
  open(): void;
}

export interface LeftDrawerProps {
  /**
   * Show when md up
   */
  mdUp: boolean;

  /**
   * Organization
   */
  organization?: number;

  /**
   * Width
   */
  width: number;
}

export const LeftDrawer = React.forwardRef<LeftDrawerMethods, LeftDrawerProps>(
  (props, ref) => {
    // Destruct
    const { mdUp, width } = props;

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
    const location = useLocation();

    const getMenuItem = (href: string) => {
      return MUGlobal.getMenuItem(location.pathname, href);
    };

    // Menu open/close state
    const [open, setOpen] = React.useState<boolean>();

    const handleDrawerClose = () => setOpen(false);

    React.useImperativeHandle(ref, () => ({
      open() {
        setOpen(true);
      }
    }));

    // Permissions
    const adminPermission = app.hasPermission([
      UserRole.Admin,
      UserRole.Founder
    ]);

    // Ready
    React.useEffect(() => {
      setOpen(mdUp);
    }, [mdUp]);

    return (
      <Drawer
        sx={{
          width,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width,
            boxSizing: 'border-box'
          }
        }}
        anchor="left"
        variant={mdUp ? 'persistent' : 'temporary'}
        open={open}
        onClose={mdUp ? undefined : handleDrawerClose}
        ModalProps={{
          keepMounted: true // Better open performance on mobile.
        }}
      >
        <DrawerHeader>
          <a
            href="https://www.etsoo.com"
            title={labels.etsoo}
            target="_blank"
            rel="noreferrer"
          >
            <Avatar
              src={process.env.PUBLIC_URL + '/logo192.png'}
              variant="square"
              sx={{ marginLeft: -0.5, marginRight: 2, marginBottom: 1 }}
            />
          </a>
          <Typography noWrap component="div" title={labels.appName}>
            {labels.appName}
          </Typography>
        </DrawerHeader>
        <Divider />
        <List onClick={mdUp ? undefined : handleDrawerClose}>
          <ListItemButton {...getMenuItem('/home/')}>
            <ListItemIcon>
              <HomeIcon />
            </ListItemIcon>
            <ListItemText primary={labels.menuHome} />
          </ListItemButton>
          <ListItemButton {...getMenuItem('/home/article/all')}>
            <ListItemIcon>
              <ArticleIcon />
            </ListItemIcon>
            <ListItemText primary={labels.articles} />
          </ListItemButton>
          <ListItemButton {...getMenuItem('/home/tab/all')}>
            <ListItemIcon>
              <TabIcon />
            </ListItemIcon>
            <ListItemText primary={labels.tabs} />
          </ListItemButton>
          <ListItemButton {...getMenuItem('/home/resource/all')}>
            <ListItemIcon>
              <ListAltIcon />
            </ListItemIcon>
            <ListItemText primary={labels.resources} />
          </ListItemButton>
          <ListItemButton {...getMenuItem('/home/config/all')}>
            <ListItemIcon>
              <SettingsIcon />
            </ListItemIcon>
            <ListItemText primary={labels.configs} />
          </ListItemButton>
          <ListItemButton {...getMenuItem('/home/plugin/all')}>
            <ListItemIcon>
              <ExtensionIcon />
            </ListItemIcon>
            <ListItemText primary={labels.plugins} />
          </ListItemButton>
          {adminPermission && (
            <ListItemButton {...getMenuItem('/home/user/all')}>
              <ListItemIcon>
                <GroupIcon />
              </ListItemIcon>
              <ListItemText primary={labels.users} />
            </ListItemButton>
          )}
        </List>
      </Drawer>
    );
  }
);
