import React from 'react';
import {
  Divider,
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  Typography
} from '@mui/material';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import { BridgeCloseButton, UserAvatar, VBox } from '@etsoo/materialui';
import LockIcon from '@mui/icons-material/Lock';
import UpgradeIcon from '@mui/icons-material/Upgrade';
import QrCode2Icon from '@mui/icons-material/QrCode2';
import { app } from '../../app/MyApp';
import { IActionResult, UserRole } from '@etsoo/appscript';
import { useNavigate } from 'react-router-dom';
import { EventWatcher, useAsyncState } from '@etsoo/react';
import LinearProgress from '@mui/material/LinearProgress';

interface UserMenuProps {
  name: string;
  avatar: string | undefined;
  smDown: boolean;
}

function QRCode() {
  // QRCode
  const [qrCode, setQRCode] = React.useState<string>();

  React.useEffect(() => {
    app.websiteApi.qrCode({ showLoading: false }).then((qrcode) => {
      if (!qrcode) return;
      setQRCode(qrcode);
    });
  }, []);

  return (
    <VBox alignItems="center">
      {qrCode == null ? (
        <LinearProgress sx={{ width: '100%' }} />
      ) : (
        <React.Fragment>
          <Typography>{app.get('mobileAccessTip')}</Typography>
          <img alt="Mobile QRCode" src={qrCode} width="360" />
        </React.Fragment>
      )}
    </VBox>
  );
}

export function UserMenu(props: UserMenuProps) {
  // Destruct
  const { name, avatar } = props;

  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'changePassword',
    'smartERP',
    'switchOrganization',
    'signout',
    'upgradeSystem',
    'operationSucceeded',
    'mobileAccess'
  );

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // User menu anchor
  const [anchorEl, setAnchorEl] = useAsyncState<HTMLButtonElement>();

  // User menu open or not
  const isMenuOpen = Boolean(anchorEl);

  // Event watcher
  const watcher = React.useRef(new EventWatcher()).current;

  // User menu
  const handleUserMenuOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = async () => {
    await setAnchorEl(undefined);
  };

  const handleClick = async (event: React.MouseEvent<HTMLDivElement>) => {
    await handleMenuClose();

    const item = (event.target as HTMLElement).closest('li[href]');
    let href: string | null;
    if (item && (href = item.getAttribute('href')) != null) {
      // Even set transitionDuration = 0, still need to wait a little bit
      // We need to create watcher as ref because of rerenderer
      watcher.add({
        type: 'transitionend',
        action: () => {
          navigate(href!);
        },
        once: true
      });
    }
  };

  // Sign out
  const handleSignout = () => {
    // Sign out
    app.signout();
  };

  // Upgrade system
  const upgradeSystem = () => {
    app.api.put<IActionResult>('Website/UpgradeSystem').then((result) => {
      if (result == null) return;
      if (result.ok) {
        app.notifier.succeed(labels.operationSucceeded);
        return;
      }
      app.alertResult(result);
    });
  };

  // Scan QRcode
  const qrcode = () => {
    app.notifier.succeed(<QRCode />, labels.mobileAccess);
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
        transitionDuration={0}
        onTransitionEnd={(event) => watcher.do(event)}
        onClose={handleMenuClose}
        onClick={handleClick}
      >
        <MenuItem href="./user/changepassword">
          <ListItemIcon>
            <LockIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{labels.changePassword}</ListItemText>
        </MenuItem>
        <MenuItem key="qrcode" onClick={qrcode}>
          <ListItemIcon>
            <QrCode2Icon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{labels.mobileAccess}</ListItemText>
        </MenuItem>
        {adminPermission && [
          <Divider key="dividerUpgrade" />,
          <MenuItem key="upgradeSystem" onClick={upgradeSystem}>
            <ListItemIcon>
              <UpgradeIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{labels.upgradeSystem}</ListItemText>
          </MenuItem>
        ]}
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
