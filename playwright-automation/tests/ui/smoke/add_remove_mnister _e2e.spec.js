const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, editorOnlineStoryPage, subscriberNavBarPage, ministerPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  editorOnlineStoryPage = masterFixture.editorOnlineStoryPage;
  ministerPage = masterFixture.ministerPage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('Scenario 4: Add and Remove Minister ', () => {
  test(`Adding and removing a minister`, async ({}) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await ministerPage.clickOnSettings();
    await ministerPage.clickOnMyMinister();
    await ministerPage.clickOnMinisterCheckbox('Adrian Dix :');
    await ministerPage.clickOnMinisterSaveButton();
    await ministerPage.clickOnMyMinisterLink();

  });
});
