const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

let page, appPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Login Data Driven Tests', () => {
  test(`Login as ${process.env.app_username}`, async ({}) => {
    await expect(page).toHaveURL(editorUrl);
    await appPage.logOut();
  });
});
