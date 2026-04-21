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
     console.log("Actions",systemSettings );
});
test.describe('@smoke System Configurations', () => {
     test(`Login as ${process.env.app_username}`, async ({page}) => {
     
    await page.goto(editorUrl);
    await systemSettings.navigateToSystemSettings();
    await systemSettings.navigatetoWorkOrder();
    
    const statusvalue = `Completed`;
    await systemSettings.searchWorkOrderbystatus(statusvalue);
    console.log(" Fetch Value:", statusvalue);
    await expect(systemSettings.tablerow).toBeVisible();
    await systemSettings.enterstatuskeyword(statusvalue);
    await systemSettings.verifyAllStatusCompleted();
    await systemSettings.NextPage();
    
    await appPage.logOut();
  });
});