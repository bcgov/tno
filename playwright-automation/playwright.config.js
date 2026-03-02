require('dotenv').config();

const { defineConfig } = require('@playwright/test');
const fs = require('fs');

// if(fs.existsSync('./allure-results')) {
//   fs.rmSync('./allure-results', { recursive: true, force: true});
// }

module.exports = defineConfig({
  testDir: './tests',
  timeout: 60000,
  retries: 0,
  workers: process.env.CI ? 4 : 1,

  reporter: [
    ['list'],
    ['allure-playwright']
  ],

  use: {
    baseURL: process.env.BASE_URL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    headless: false,
    launchOptions: {
       args:['--start-maximized']
    },
    viewport: null
  },

  projects: [
    { name: 'chromium', use: { browserName: 'chromium' } },
    // { name: 'firefox', use: { browserName: 'firefox' } }
  ]
});