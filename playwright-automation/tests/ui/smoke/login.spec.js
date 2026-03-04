const { test, expect } = require('../../../fixtures/ui-fixture');
const AppPage = require('../../../pages/appPage');
const env = require('../../../config/env.config');
const DataLoader = require('../../../utils/dataLoader');

const users = DataLoader.loadJSON('test-data/loginData.json');


test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test.describe('@smoke Login Data Driven Tests', () => {

    test(`Login as ${env.username}`, async ({ page }) => {
      const appPage = new AppPage(page);
      await appPage.login(env.username, env.password);
      await expect(page).toHaveURL('/contents');
    });

});