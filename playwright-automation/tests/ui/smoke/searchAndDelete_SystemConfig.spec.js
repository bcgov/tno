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
  test(`Search and Delete System Configuration`, async ({ page }) => {
    await page.goto(editorUrl);
    await systemSettings.navigateToSystemSettings();
    await systemSettings.navigatetoSystemConfig();

    const KeyValue = `Automation Test System`;

    await systemSettings.searchSystemConfig(KeyValue);
    console.log(' Fetch Value:', KeyValue);
    await systemSettings.deleteSystemConfig();
    console.log(`Deletion of ${KeyValue} is successful.`);
    await systemSettings.confirmDelete();
    await systemSettings.NavigatetoSettings();
    console.log('Navigate to System Settings');

    //Validate the delete message
    await systemSettings.validateDeleteToastMessage();
    console.log(`Deletion of ${KeyValue} is successful.`);
    await appPage.logOut();
  });
});
