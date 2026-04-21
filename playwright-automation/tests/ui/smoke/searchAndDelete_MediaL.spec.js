const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];
const CONSTANTS = require('../../../utils/constants');

let page, appPage, dataImport;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  dataImport = masterFixture.dataImport;
  await appPage.navigateToUrl(editorUrl);
    await appPage.hardWait(5000);
     console.log("Actions",dataImport );
});
test.describe('@smoke Media Licenses', () => {
     test(`Login as ${process.env.app_username}`, async ({page}) => {
     
    await page.goto(editorUrl);
    await dataImport.navigateToDataImport();
    await dataImport.navigatetoMediaL();

    const LicenseName = `Automation Test Media Licenses`;

    await dataImport.searchbox(LicenseName);
     console.log(" Fetch Value:", LicenseName);
     
      await dataImport.clickDelete();
      await dataImport.removeBtn(); 
       await appPage.logOut();
  });
});