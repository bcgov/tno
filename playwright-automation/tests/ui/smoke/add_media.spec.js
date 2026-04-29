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
test.describe('@smoke Add Media TYPE', () => {
     test(`Verify adding new media type`, async ({page}) => {
     
    await page.goto(editorUrl);
    await addMediaPage.navigateToCC();
    await addMediaPage.navigateToMiMedia();
    await addMediaPage.clickAddNewMedia();
    
    const randomNumber = Math.floor(Math.random() * 10000)+1;
    const MediaName = `Automation Test Name ${randomNumber}`;
    const MediaDescription = `Automation Test Description`;
    const MediaSortOrder = `${randomNumber}`;

    await addMediaPage.enterMediaDetails(
      MediaName,
      MediaDescription,
      MediaSortOrder
    );

    await addMediaPage.selectListTypeOption('Program/Show');   

    //Validate data is saved
    await expect(await addMediaPage.validateSuccessMessage()).toBe(true);
    await addMediaPage.backbtnMedia();
    await appPage.logOut();
  });
});