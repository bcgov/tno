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
});
test.describe('@smoke Add New Actions', () => {
  test(`Verify user can Add new action successfully `, async ({ page }) => {
    await page.goto(editorUrl);
    await addMediaPage.navigateToCC();
    await addMediaPage.navigateToActions();
    await addMediaPage.clickAddNewActions();

    const MediaName = `Automation Test Name`;
    const MediaDescription = `Description for Action Name`;
    const randomNumber = Math.floor(Math.random() * 100) + 1;
    const MediaSortOrder = `${randomNumber}`;

    await addMediaPage.enterMediaDetails(MediaName, MediaDescription, MediaSortOrder);

    await addMediaPage.contentdropdown('Print Content');

    //Validate data is saved
    await expect(await addMediaPage.validateSuccessMessage()).toBe(true);

    await addMediaPage.clickBackToActions();
    await appPage.logOut();
  });
});
