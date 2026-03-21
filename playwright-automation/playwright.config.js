require('dotenv').config();

const { defineConfig } = require('@playwright/test');
const fs = require('fs');

// if(fs.existsSync('./allure-results')) {
//   fs.rmSync('./allure-results', { recursive: true, force: true});
// }

module.exports = defineConfig({
  testDir: './tests',
  globalSetup: './utils/global-setup.js',
  timeout: 60000,
  retries: 0,
  workers: process.env.CI ? 4 : 2,

  reporter: [['list'], ['html', { outputFolder: 'playwright-report' }], ['allure-playwright']],

  use: {
    ignoreHTTPSErrors: true,
    launchOptions: {
      args: ['--start-maximized'],
    },
    actionTimeout: 30000,
    trace: 'off',
    screenshot: 'on',
    storageState: '/tmp/login.json',
    headless: false,
    viewport: null,
    launchOptions: {
      args: ['--start-maximized'],
    },
  },

  projects: [
    { name: 'chromium', use: { browserName: 'chromium' } },
    // { name: 'firefox', use: { browserName: 'firefox' } }
  ],
  outputDir: '/tmp/test-result',
});