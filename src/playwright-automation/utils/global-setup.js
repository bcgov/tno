const { chromium } = require('@playwright/test');
const { loadEnv, requireEnv } = require('./env');
const { restrictToLocal, validateLocalTargets } = require('./local-only');
const { AppPage } = require('../pages/appPage');

async function globalSetup() {
  loadEnv();
  requireEnv(['EDITOR_URL', 'APP_USERNAME', 'APP_PASSWORD']);
  validateLocalTargets();
  const browser = await chromium.launch();
  try {
    const context = await browser.newContext({ viewport: { width: 1920, height: 1080 }, serviceWorkers: 'block' });
    await restrictToLocal(context);
    const page = await context.newPage();
    try {
      await new AppPage(page).navigateToUrl(process.env.LOGIN_URL);
    } catch (error) {
      if (new URL(page.url()).pathname.includes('/login-actions/required-action')) {
        throw new Error('Keycloak requires account profile completion before browser tests can run. Complete the required fields for APP_USERNAME in the local browser login.', { cause: error });
      }
      throw error;
    }
    await context.storageState({ path: '/tmp/login.json' });
  } finally {
    await browser.close();
  }
}

module.exports = globalSetup;
