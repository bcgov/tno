const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];
const CONSTANTS = require('../../../utils/constants');

let page, appPage, systemSettings;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  systemSettings = masterFixture.systemSettings;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(5000);
  console.log('Actions', systemSettings);
});
test.describe('@smoke System Configurations', () => {
  test(`Verify Search by Role for Manage Users`, async ({ page }) => {
    await page.goto(editorUrl);
    await systemSettings.navigateToSystemSettings();
    await systemSettings.navigatetoManageUser();

    const role = `editor`;
    await systemSettings.searchManageUserbyRole(role);
    console.log(' Fetch Value:', role);

    await systemSettings.searchWorkOrder();
    console.log('Clicked on Search button');
    // Verify that all the role values in the search results are "Editor"
    await systemSettings.tableRoleCol();
    console.log('All role values are Editor');
    await appPage.logOut();
  });
});
