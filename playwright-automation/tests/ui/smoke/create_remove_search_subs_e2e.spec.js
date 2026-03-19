const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];
const folderName = 'CreateFolderPlaywrightTest';
const searchText = 'Playwright Test Search Text';

let page, appPage, settingsPage, ministerPage;
test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  settingsPage = masterFixture.settingsPage;
  ministerPage = masterFixture.ministerPage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Create Search Folder', () => {
  test(`Creating a search and verifying saved search `, async ({}) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await ministerPage.clickOnSettings();
    await settingsPage.clickOnAdvancedSearchLink();
    await settingsPage.enterAdvancedSearchText(searchText);
    await settingsPage.enterAdvancedSearchName(folderName);
    await settingsPage.clickOnSaveSearchButton();
    await settingsPage.clickOnSavedSearchesLink();
    await settingsPage.clearSearchInputText();
    await expect(page.getByText(folderName)).toBeVisible();
     });
});
