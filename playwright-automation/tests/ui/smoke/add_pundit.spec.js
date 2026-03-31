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
     console.log("TAGS",addMediaPage );
});
test.describe('@smoke Add TAGS', () => {
     test(`Login as ${process.env.app_username}`, async ({page}) => {
     
    await page.goto(editorUrl);
    await addMediaPage.navigateToCC();
    await addMediaPage.navigateToColumn_Pundit();
    await addMediaPage.clickAddNewPundit();
    
    const PunditName = `Test Automation Pundit`;
    //const PunditAliases = `Autotest Name`;
    const PunditDescriptionName = `Automation Test Description`;
     const randomNumber = Math.floor(Math.random() * 100)+1;
    const PunditNamertOrder = `${randomNumber}`;

    await addMediaPage.enterMediaDetails(
      PunditName,
      PunditDescriptionName,
      PunditNamertOrder
    );

    await addMediaPage.selectListOptValue('Penticton Herald (PH)');
   
    await page.waitForTimeout(10000);
    await addMediaPage.clickBackToColumn_pundit();
    await page.waitForTimeout(2000);

    await addMediaPage.searchAndValidation(PunditName);
    console.log("Fetch Value:", PunditName);

   await addMediaPage.clickOnVisibledata();
   await addMediaPage.clickOnDelete();
   await addMediaPage.removeData();
   
 await appPage.logOut();
});
});
