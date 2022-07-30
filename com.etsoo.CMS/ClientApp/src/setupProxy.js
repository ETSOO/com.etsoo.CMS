const { createProxyMiddleware } = require('http-proxy-middleware');
const { env } = require('process');

/**
 * Proxying API Requests in Development
 * https://create-react-app.dev/docs/proxying-api-requests-in-development/
 */

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
  ? env.ASPNETCORE_URLS.split(';')[0]
  : 'http://localhost:38573';

module.exports = function (app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target,
      changeOrigin: true,
      secure: false,
      headers: {
        Connection: 'Keep-Alive'
      }
    })
  );
};
