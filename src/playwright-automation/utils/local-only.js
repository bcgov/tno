const { boolEnv } = require('./env');

function isLoopback(url) {
  const parsed = new URL(url);
  return ['http:', 'https:', 'ws:', 'wss:'].includes(parsed.protocol) &&
    ['localhost', '127.0.0.1', '[::1]'].includes(parsed.hostname);
}

function validateLocalTargets() {
  if (!boolEnv('LOCAL_ONLY')) return;
  for (const key of ['EDITOR_URL', 'SUBSCRIBER_URL', 'API_BASE_URL', 'LOGIN_URL']) {
    if (!process.env[key] || !isLoopback(process.env[key])) {
      throw new Error(`LOCAL_ONLY requires a loopback URL for ${key}`);
    }
  }
}

async function restrictToLocal(context) {
  if (!boolEnv('LOCAL_ONLY')) return;
  validateLocalTargets();
  await context.route('**/*', async (route) => {
    if (isLoopback(route.request().url())) return route.continue();
    console.error(`LOCAL_ONLY blocked request to ${new URL(route.request().url()).origin}`);
    return route.abort('blockedbyclient');
  });
  await context.routeWebSocket('**/*', (socket) => {
    if (isLoopback(socket.url())) socket.connectToServer();
    else socket.close();
  });
}

module.exports = { isLoopback, restrictToLocal, validateLocalTargets };
