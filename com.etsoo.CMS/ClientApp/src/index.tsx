import React from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App';
import { app, NotifierProvider } from './app/MyApp';
import {
  createTheme,
  CssBaseline,
  LinearProgress,
  ThemeProvider
} from '@mui/material';
import * as locales from '@mui/material/locale';
import { HRouter } from '@etsoo/react';
import { Route, Routes } from 'react-router-dom';
import Dashboard from './main/home/Dashboard';
import Home from './main/home/Home';

// Lazy load components
const ChangePassword = React.lazy(() => import('./main/user/ChangePassword'));

// Culture provider
const CultureStateProvider = app.cultureState.provider;
const CultureContext = app.cultureState.context;

// All supported locales of the UI framework
type SupportedLocales = keyof typeof locales;

// User state
const UserStateProvider = app.userState.provider;

// Page state
const PageStateProvider = app.pageState.provider;

// Theme
// https://mui.com/customization/theming/
// https://material.io/resources/color
const theme = createTheme({
  palette: {
    primary: {
      main: '#3f51b5'
    }
  },
  components: {
    MuiCardContent: {
      styleOverrides: {
        root: {
          // other styles
          '&:last-child': {
            paddingBottom: '16px'
          }
        }
      }
    },
    MuiFormLabel: {
      styleOverrides: {
        asterisk: {
          color: '#db3131',
          '&$error': {
            color: '#ff0000'
          }
        }
      }
    }
  }
});

function MyRouter() {
  // Init state
  const [init, setInit] = React.useState(false);

  // Ready
  React.useEffect(() => {
    // Persist app data
    const cleanup = () => {
      app.persist();
    };

    window.addEventListener('unload', cleanup);
    window.addEventListener('beforeunload', cleanup);

    // Init call
    app.initCall((result) => {
      setInit(result);
    });
  }, []);

  return init ? (
    // Need new solution for flicker
    <React.Suspense fallback={<LinearProgress />}>
      <HRouter basename={app.settings.homepage} history={app.history}>
        <Routes>
          <Route path="*" element={<App />} />

          <Route path="/home" element={<Home />}>
            <Route index element={<Dashboard />} />

            <Route path="user/changepassword" element={<ChangePassword />} />
          </Route>
        </Routes>
      </HRouter>
    </React.Suspense>
  ) : (
    <React.Fragment />
  );
}

const reactRoot = createRoot(document.getElementById('root')!);
reactRoot.render(
  <ThemeProvider theme={theme}>
    <CultureStateProvider>
      <CultureContext.Consumer>
        {(culture) => (
          <ThemeProvider
            theme={(outerTheme) =>
              createTheme(
                outerTheme,
                locales[culture.state.name.replace('-', '') as SupportedLocales]
              )
            }
          >
            <NotifierProvider />
            <UserStateProvider
              update={(dispatch) => {
                app.userStateDispatch = dispatch;
              }}
            >
              <PageStateProvider
                update={(dispatch) => {
                  app.pageStateDispatch = dispatch;
                }}
              >
                <CssBaseline />
                <MyRouter />
              </PageStateProvider>
            </UserStateProvider>
          </ThemeProvider>
        )}
      </CultureContext.Consumer>
    </CultureStateProvider>
  </ThemeProvider>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
/*
if (process.env.NODE_ENV !== 'production') {
  reportWebVitals(console.log);
}
*/
