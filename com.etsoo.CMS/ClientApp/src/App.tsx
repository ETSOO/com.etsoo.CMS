import { DomUtils } from '@etsoo/shared';
import React from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { app } from './app/MyApp';

function App() {
  // Route
  const navigate = useNavigate();

  const [search] = useSearchParams();

  // Queries
  const params = DomUtils.dataAs(search, {
    tryLogin: 'string',
    token: 'string'
  });

  const tryLogin = params.tryLogin;

  // Token
  const token = params.token;
  if (token) {
    // Cache the service token to local refresh token
    app.storage.setData(app.fields.headerToken, app.encrypt(token));
  }

  React.useEffect(() => {
    // Try login
    if (tryLogin === 'false') return;

    app.tryLogin(undefined, true).then((result) => {
      if (result) {
        navigate('./home/');
        return;
      }
    });
  }, [navigate, tryLogin]);

  return <React.Fragment></React.Fragment>;
}

export default App;
