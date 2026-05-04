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
     test(`Search and Delete Media License`, async ({page}) => {
     
    await page.goto(editorUrl);
    await dataImport.navigateToDataImport();
    await dataImport.navigatetoMediaL();

    const LicenseName = `Automation Test data`;

    await dataImport.searchboxValue(LicenseName);
     console.log(" Fetch Value:", LicenseName);
     await dataImport.clickRow();
      console.log(`Row value validation for ${LicenseName} is successful.`);
      await dataImport.clickDelete();
    console.log(`Deletion of ${LicenseName} is successful.`);
    await dataImport.removeBtn(); 
      await dataImport.navigatetoMediaLicense();
      console.log("Navigate to Data Locations");

     //Validate the delete message
     await dataImport.validateDeleteMessage();
      console.log(`Deletion of ${LicenseName} is successful.`)
       await appPage.logOut();
  });
});