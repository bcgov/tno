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
    await systemSettings.navigatetoSystemConfig();
    await systemSettings.clickAddNewSystemConfig();
    
    const SCName = `Automation Test System Configuration`;
    const SCDescription = `Automation Description Data for testing `;
    const SCrandomNmb = Math.floor(Math.random() * 100)+1;
    const SCSortOrder = `${SCrandomNmb}`;
    const SCValue = `Automation test Value for System configuration`;

    await systemSettings.enterSystemConfigDetails(
      SCName,
      SCDescription,
      SCValue,
      SCSortOrder  
    );
    
    await appPage.logOut();
  });
});