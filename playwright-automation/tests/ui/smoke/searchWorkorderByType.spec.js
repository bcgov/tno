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
    
    const KeyType = `Transcription`;
    await systemSettings.searchWorkOrder(KeyType);
    console.log(" Fetch Value:", KeyType);
    await systemSettings.enterSearchKeyword(KeyType);
    await systemSettings.SaveWorkOrder();
    await systemSettings.backtoWorkorder();
    await appPage.logOut();
  });
});