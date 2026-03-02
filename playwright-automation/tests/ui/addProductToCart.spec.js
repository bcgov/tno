const { test, expect } = require('../../fixtures/ui-fixture');
const DashboardPage = require('../../pages/DashboardPage');
const CONSTANTS = require('../../utils/constants');
const logger = require('../../utils/logger');

test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test('@smoke Add product to cart', async ({ page }) => {

  logger.info('Starting cart test');

  const dashboard = new DashboardPage(page);

  await dashboard.verifyDashboardLoaded();
  await dashboard.searchProduct('Laptop');
  await dashboard.addFirstProductToCart();

  const count = await dashboard.getCartCount();

  logger.info(`Cart Count: ${count}`);

  expect(Number(count)).toBeGreaterThan(0);
});

test.afterEach(async ({ page }, testInfo) => {
  if (testInfo.status !== testInfo.expectedStatus) {
    await page.screenshot({ path: `screenshots/${testInfo.title}.png` });
  }
});