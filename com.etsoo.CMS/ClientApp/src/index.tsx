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
import { Route, Routes } from 'react-router-dom';
import Dashboard from './main/home/Dashboard';
import Home from './main/home/Home';
import AllArticles from './main/article/AllArticles';
import AddArticle from './main/article/AddArticle';
import { DynamicRouter } from '@etsoo/react';
import { zhCN, zhHK } from '@mui/material/locale';

// Lazy load components
const AllTabs = React.lazy(() => import('./main/tab/AllTabs'));
const AddTab = React.lazy(() => import('./main/tab/AddTab'));
const TabLogo = React.lazy(() => import('./main/tab/TabLogo'));

const ViewArticle = React.lazy(() => import('./main/article/ViewArticle'));
const ArticleLogo = React.lazy(() => import('./main/article/ArticleLogo'));
const ArticleGallery = React.lazy(
  () => import('./main/article/ArticleGallery')
);

const ChangePassword = React.lazy(() => import('./main/user/ChangePassword'));
const AllUsers = React.lazy(() => import('./main/user/AllUsers'));
const AddUser = React.lazy(() => import('./main/user/AddUser'));
const UserHistory = React.lazy(() => import('./main/user/UserHistory'));

const Settings = React.lazy(() => import('./main/website/Settings'));
const Plugins = React.lazy(() => import('./main/website/Plugins'));
const Resources = React.lazy(() => import('./main/website/Resources'));
const OnlineDrive = React.lazy(() => import('./main/website/OnlineDrive'));

// Culture provider
const CultureStateProvider = app.cultureState.provider;
const CultureContext = app.cultureState.context;

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
    const init = () => {
      app.initCall((result) => {
        setInit(result);
      });
    };
    if (app.isReady) {
      init();
    } else {
      app.pendings.push(init);
    }
  }, []);

  const basename = app.settings.homepage;

  return init ? (
    // Need new solution for flicker
    <React.Suspense fallback={<LinearProgress />}>
      <DynamicRouter basename={basename}>
        <Routes>
          <Route path="*" element={<App />} />

          <Route path="home" element={<Home />}>
            <Route index element={<Dashboard />} />

            <Route path="article/all" element={<AllArticles />} />
            <Route path="article/add" element={<AddArticle />} />
            <Route path="article/edit/:id" element={<AddArticle />} />
            <Route path="article/view/:id" element={<ViewArticle />} />
            <Route path="article/logo/:id" element={<ArticleLogo />} />
            <Route path="article/gallery/:id" element={<ArticleGallery />} />

            <Route path="tab/all" element={<AllTabs />} />
            <Route path="tab/add" element={<AddTab />} />
            <Route path="tab/edit/:id" element={<AddTab />} />
            <Route path="tab/logo/:id" element={<TabLogo />} />

            <Route path="user/changepassword" element={<ChangePassword />} />
            <Route path="user/all" element={<AllUsers />} />
            <Route path="user/edit/:id" element={<AddUser />} />
            <Route path="user/add" element={<AddUser />} />
            <Route path="user/history/:id" element={<UserHistory />} />

            <Route path="resource/all" element={<Resources />} />
            <Route path="resource/onlinedrive" element={<OnlineDrive />} />
            <Route path="config/all" element={<Settings />} />
            <Route path="plugin/all" element={<Plugins />} />
          </Route>
        </Routes>
      </DynamicRouter>
    </React.Suspense>
  ) : (
    <React.Fragment />
  );
}

const getThemeCulture = (name: string) => {
  switch (name) {
    case 'zh-Hans':
      return zhCN;
    case 'zh-Hant':
      return zhHK;
  }
  return {};
};

const reactRoot = createRoot(document.getElementById('root')!);
reactRoot.render(
  <ThemeProvider theme={theme}>
    <CultureStateProvider>
      <CultureContext.Consumer>
        {(culture) => (
          <ThemeProvider
            theme={createTheme(theme, getThemeCulture(culture.state.name))}
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
