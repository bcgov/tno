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
     console.log("Media Page",addMediaPage );
});
test.describe('@smoke Search and Media TYPE', () => {
     test(`Login as ${process.env.app_username}`, async ({page}) => {
     
    await page.goto(editorUrl);
    await addMediaPage.navigateToCC();
    await addMediaPage.navigateToMiMedia(); 
    await page.waitForTimeout(5000);

    //const uniqueID = Date.now();
    const mediaName = `Automation Media type Name`;
    //const mediadescription = `Automation Media type Name - ${uniqueID}`;
     await addMediaPage.searchAndValidation(mediaName);
     console.log(" Fetch Value:", mediaName);
     
      await addMediaPage.clickOnVisibleText();
      await addMediaPage.clickOnDelete();
      await addMediaPage.removeData();
      await appPage.logOut();
  });
});