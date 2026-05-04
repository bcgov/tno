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
     test(`Verify Search by Status for Manage Users`, async ({page}) => {
     
    await page.goto(editorUrl);
    await systemSettings.navigateToSystemSettings();
    await systemSettings.navigatetoManageUser();
    
    const status = `Approved`;
    await systemSettings.searchWorkUserbystatus(status);
    console.log(" Fetch Value:", status);

    await systemSettings.searchWorkOrder();
    console.log("Clicked on Search button");
 

  // Verify that all the status values in the search results are "Approved"
    await systemSettings.tableStatusCol();
    console.log("All status values are Approved");
    await appPage.logOut();
  });
});