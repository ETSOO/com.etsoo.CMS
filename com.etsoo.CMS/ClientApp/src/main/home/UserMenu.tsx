import React from 'react';
import {
  Divider,
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem
} from '@mui/material';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import { BridgeCloseButton, RLink, UserAvatar } from '@etsoo/react';
import LockIcon from '@mui/icons-material/Lock';
import { app } from '../../app/MyApp';

interface UserMenuProps {
  organization: number | undefined;
  name: string;
  avatar: string | undefined;
  smDown: boolean;
}

export function UserMenu(props: UserMenuProps) {
  // Destruct
  const { organization, name, avatar, smDown } = props;

  // Labels
  const labels = app.getLabels(
    'changePassword',
    'smartERP',
    'switchOrganization',
    'signout'
  );

  // User menu anchor
  const [anchorEl, setAnchorEl] = React.useState<HTMLButtonElement>();

  // User menu open or not
  const isMenuOpen = Boolean(anchorEl);

  // User menu
  const handleUserMenuOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(undefined);
  };

  // Sign out
  const handleSignout = () => {
    // Close menu
    setAnchorEl(undefined);

    // Sign out
    app.signout();
  };

  return (
    <React.Fragment>
      <IconButton
        edge="end"
        aria-haspopup="true"
        onClick={handleUserMenuOpen}
        color="inherit"
      >
        <UserAvatar title={name} src={avatar} />
      </IconButton>
      <BridgeCloseButton
        color="secondary"
        boxProps={{
          sx: {
            marginLeft: 1.5,
            marginRight: -1.5
          }
        }}
      />
      <Menu
        PaperProps={{
          elevation: 0,
          sx: {
            overflow: 'visible',
            filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.32))',
            mt: -0.4,
            '& .MuiAvatar-root': {
              width: 32,
              height: 32,
              ml: -0.5,
              mr: 1
            },
            '&:before': {
              content: '""',
              display: 'block',
              position: 'absolute',
              top: 0,
              right: 14,
              width: 10,
              height: 10,
              bgcolor: 'background.paper',
              transform: 'translateY(-50%) rotate(45deg)',
              zIndex: 0
            }
          }
        }}
        disableScrollLock
        anchorEl={anchorEl}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right'
        }}
        keepMounted
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right'
        }}
        open={isMenuOpen}
        onClick={handleMenuClose}
        onClose={handleMenuClose}
      >
        <MenuItem component={RLink} href="/home/user/changepassword">
          <ListItemIcon>
            <LockIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{labels.changePassword}</ListItemText>
        </MenuItem>
        <Divider />
        <MenuItem onClick={handleSignout}>
          <ListItemIcon>
            <ExitToAppIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{labels.signout}</ListItemText>
        </MenuItem>
      </Menu>
    </React.Fragment>
  );
}
