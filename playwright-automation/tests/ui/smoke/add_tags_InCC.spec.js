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
    await addMediaPage.navigateToTAGS();
    await addMediaPage.clickAddNewTag();
    
    const TagCode = `AGG Auto `;
    const TagName = `Autotest Name`;
    const DescriptionTag = `Automation Test Description`;
     const randomNumber = Math.floor(Math.random() * 100)+1;
    const TagSortOrder = `${randomNumber}`;

    await addMediaPage.enterTagDetails(
      TagCode,
      TagName,
      DescriptionTag,
      TagSortOrder
    );

    await page.waitForTimeout(10000);
    await addMediaPage.clickBackToTAG();
    await page.waitForTimeout(2000);

    await addMediaPage.searchTagValue(TagCode);
     console.log(" Fetch Value:", TagCode);

   await addMediaPage.clickOnVisiblecode();
   await addMediaPage.clickOnDelete();
   await addMediaPage.removeData();
   await appPage.logOut();
  });
});