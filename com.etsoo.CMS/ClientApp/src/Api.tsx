import { DomUtils } from '@etsoo/shared';
import React from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { app } from './app/MyApp';

function Api() {
  // Route
  const navigate = useNavigate();
  const [search] = useSearchParams();

  // Queries
  const params = DomUtils.dataAs(search, { token: 'string' });

  // Provider, not used
  // Token
  const token = params.token;
  if (token) {
    // Cache the service token to local refresh token
    app.storage.setData(app.fields.headerToken, app.encrypt(token));
  }

  React.useEffect(() => {
    // Try login
    app.tryLogin(undefined, true).then((result) => {
      if (result) {
        navigate('/home/');
        return;
      }
    });
  }, [navigate]);

  return <React.Fragment></React.Fragment>;
}

export default Api;
