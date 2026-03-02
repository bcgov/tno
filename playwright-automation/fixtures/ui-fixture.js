const { test: base, expect } = require('@playwright/test');
const ENV = require('../config/env.config');
const logger = require('../utils/logger');

const test = base.extend({
  page: async ({ page }, use, testInfo) => {
    logger.info(`Starting Test: ${testInfo.title}`);

    await page.goto(ENV.baseURL);

    await use(page);

    logger.info(`Finished Test: ${testInfo.title}`);
  },
});

/**
 * Global afterEach for all UI tests.
 */
test.afterEach(async ({ page }, testInfo) => {
  // If test failed
  if (testInfo.status !== testInfo.expectedStatus) {
    logger.error(`Test Failed: ${testInfo.title}`);

    // Attach screenshot directly to eport
    await testInfo.attach('Failure Screenshot', {
      body: await page.screenshot(),
      contentType: 'img/png',
    });
  }
});

module.exports = {
  test,
  expect,
};
