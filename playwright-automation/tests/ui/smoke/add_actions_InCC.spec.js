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
     console.log("Actions",addMediaPage );
});
test.describe('@smoke Add New Actions', () => {
     test(`Login as ${process.env.app_username}`, async ({page}) => {
     
    await page.goto(editorUrl);
    await addMediaPage.navigateToCC();
    await addMediaPage.navigateToActions();
    await addMediaPage.clickAddNewActions();
    
    const MediaName = `Automation Action Name`;
    const MediaDescription = `Automation Action Description`;
    const randomNumber = Math.floor(Math.random() * 100)+1;
    const MediaSortOrder = `${randomNumber}`;

    await addMediaPage.enterMediaDetails(
      MediaName,
      MediaDescription,
      MediaSortOrder
    );

    await addMediaPage.contentdropdown('Print Content');  
    
    await addMediaPage.clickBackToActions();
     const mediaName = `Automation Action Name`;
    //const mediadescription = `Automation Media type Name - ${uniqueID}`;
     await addMediaPage.searchAndValidation(mediaName);
     console.log(" Fetch Value:", mediaName);
     
      await addMediaPage.clickOnSelectedValue();
      await addMediaPage.clickOnDelete();
      await addMediaPage.removeData();
    await appPage.logOut();
  });
});