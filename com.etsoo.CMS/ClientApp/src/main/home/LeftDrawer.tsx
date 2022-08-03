import {
  Avatar,
  Divider,
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  styled,
  Typography,
  useTheme
} from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import React from 'react';
import { CSSProperties } from 'react';
import { UserRole } from '@etsoo/appscript';
import { app } from '../../app/MyApp';
import { Link, useLocation } from 'react-router-dom';
import { RLink } from '@etsoo/react';

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
    const labels = app.getLabels('etsoo', 'menuHome', 'appName');

    // Location
    // Reload when location changes
    const location = useLocation();

    const theme = useTheme();

    // Menu properties
    const getMenuItem = (href: string) => {
      const path = location.pathname;
      const asterisk = href.endsWith('*');
      if (asterisk) href = href.slice(0, -1);
      const selected = asterisk ? path.startsWith(href) : href === path;

      return {
        component: RLink,
        selected,
        href,
        sx: {
          ...(selected && {
            '.MuiListItemIcon-root': { color: theme.palette.primary.main }
          })
        }
      };
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
    const financePermission = app.hasPermission([
      UserRole.Finance,
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
          <ListItem button {...getMenuItem('/home/')}>
            <ListItemIcon>
              <HomeIcon />
            </ListItemIcon>
            <ListItemText primary={labels.menuHome} />
          </ListItem>
        </List>
      </Drawer>
    );
  }
);
