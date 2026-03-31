const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];
const CONSTANTS = require('../../../utils/constants');

let page, appPage, addMediaPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  addMediaPage = masterFixture.addMediaPage;
  await appPage.navigateToUrl(editorUrl);
    await appPage.hardWait(5000);
     console.log("Ingest Type",addMediaPage );
});
test.describe('@smoke Add TAGS', () => {
     test(`Login as ${process.env.app_username}`, async ({page}) => {
     
    await page.goto(editorUrl);
    await addMediaPage.navigateToDataImport();
    await addMediaPage.navigateToIngestType();
    await addMediaPage.clickAddNewIngestType();
    
    const IngestName = `Test Automation IT`;
    const IngestDescriptionName = `Automation Test Description`;
     const randomNumber = Math.floor(Math.random() * 100)+1;
    const IngestNamertOrder = `${randomNumber}`;

    await addMediaPage.enterMediaDetails(
      IngestName,
      IngestDescriptionName,
      IngestNamertOrder
    );

    await addMediaPage.selectListOption('Internet');
   
    await page.waitForTimeout(10000);
    await addMediaPage.clickBackToIngestType();
    await page.waitForTimeout(2000);

    await addMediaPage.searchAndValidation(IngestName);
    console.log("Fetch Value:", IngestName);

   await addMediaPage.clickOnSearchValue();
   await addMediaPage.clickOnDelete();
   await addMediaPage.removeData();
 await appPage.logOut();
});
});
