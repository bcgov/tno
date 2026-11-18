const { defineConfig } = require('@playwright/test');
const { boolEnv, loadEnv, numberEnv } = require('./utils/env');

loadEnv();

const isCI = boolEnv('CI');
const isHeadless = boolEnv('HEADLESS');
const actionTimeout = numberEnv('ACTION_TIMEOUT', 30000);
const testTimeout = numberEnv('TEST_TIMEOUT', 300000);

// if(fs.existsSync('./allure-results')) {
//   fs.rmSync('./allure-results', { recursive: true, force: true});
// }

module.exports = defineConfig({
  testDir: './tests',
  globalSetup: './utils/global-setup.js',
  timeout: testTimeout,
  retries: isCI ? 2 : 1,
  workers: isCI ? 1 : 1,

  reporter: [
    ['list'],
    ['allure-playwright'],
  ],

  use: {
    ignoreHTTPSErrors: true,
    actionTimeout,
    trace: 'off',
    screenshot: 'on',
    storageState: '/tmp/login.json',
    headless: isHeadless,
    viewport: isHeadless ? { width: 1920, height: 1080 } : null,
    launchOptions: {
      args: isHeadless ? [] : ['--start-maximized'],
    },
  },

  projects: [
    { name: 'chromium', use: { browserName: 'chromium' } },
    // { name: 'firefox', use: { browserName: 'firefox' } }
  ],
  outputDir: '/tmp/test-result',
});
