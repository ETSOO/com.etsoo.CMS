import {
  AppBar,
  Box,
  IconButton,
  Theme,
  Toolbar,
  Typography,
  useMediaQuery
} from "@mui/material";
import React from "react";
import MenuIcon from "@mui/icons-material/Menu";
import { app } from "../../app/MyApp";
import { UserMenu } from "./UserMenu";
import { Outlet } from "react-router-dom";
import { LeftDrawerLocal } from "./LeftDrawerLocal";
import { DrawerHeader } from "@etsoo/materialui";

// Size
const width = 220;

function Home() {
  // Page context
  const PageContext = app.pageState.context;

  // User context / state
  const { state } = React.useContext(app.userState.context);
  if (state == null) {
    return <React.Fragment />;
  }

  // Theme
  const smDown = useMediaQuery<Theme>((theme) => theme.breakpoints.down("sm"));
  app.smDown = smDown;

  const mdUp = useMediaQuery<Theme>((theme) => theme.breakpoints.up("md"));
  app.mdUp = mdUp;

  const { authorized } = state;

  const [open, setOpen] = React.useState(mdUp);
  React.useEffect(() => {
    setOpen(mdUp);
  }, [mdUp]);

  // When unauthorized (by refresh)
  // Return blank and try login
  React.useEffect(() => {
    if (!authorized) app.tryLogin();
  }, [authorized]);

  if (!authorized) {
    return <React.Fragment />;
  }

  return (
    <React.Fragment>
      <AppBar
        position="fixed"
        sx={{ ...(mdUp && { paddingLeft: `${width}px` }) }}
      >
        <Toolbar>
          <IconButton
            edge="start"
            color="inherit"
            onClick={() => setOpen(true)}
            sx={{ ...(open && { display: "none" }) }}
          >
            <MenuIcon />
          </IconButton>
          <PageContext.Consumer>
            {({ state }) => (
              <React.Fragment>
                <Typography variant="h6" noWrap component="div">
                  {state.title}
                </Typography>
                {state.subtitle && (
                  <Typography
                    variant="caption"
                    noWrap
                    sx={{ marginLeft: "8px", marginTop: "8px" }}
                  >
                    {state.subtitle}
                  </Typography>
                )}
              </React.Fragment>
            )}
          </PageContext.Consumer>
          <Box sx={{ flexGrow: 1 }} />
          <UserMenu name={state.name} avatar={state.avatar} smDown={smDown} />
        </Toolbar>
      </AppBar>
      <Box sx={{ display: "flex" }}>
        <LeftDrawerLocal
          mdUp={mdUp}
          organization={state.organization}
          width={width}
          onMinimize={() => setOpen(false)}
          open={open}
        />
        {/*
        https://stackoverflow.com/questions/36247140/why-dont-flex-items-shrink-past-content-size
        */}
        <Box sx={{ flexGrow: 1, minWidth: 0 }}>
          <DrawerHeader />
          <Outlet />
        </Box>
      </Box>
    </React.Fragment>
  );
}

export default Home;
