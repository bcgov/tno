const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, editorOnlineStoryPage, subscriberNavBarPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  editorOnlineStoryPage = masterFixture.editorOnlineStoryPage;
  subscriberNavBarPage = masterFixture.subscriberNavBarPage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smokeCreate/Delete Folder ', () => {
  test(`Creating and deleting a folder`, async ({}) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await subscriberNavBarPage.clickOnMyFolder();
    const randomNum = Math.floor(Math.random() * 10000);
    const folderName = `Test Folder - ${randomNum}`;
    await subscriberNavBarPage.enterFolderName(folderName);
    await subscriberNavBarPage.clickOnCreateNewFolder();
    console.log(`Created Folder Name: ${folderName}`);
    const folders = page.locator('[class="folder-name"]', { hasText: folderName });
    await expect(folders).toBeVisible();
    await page.locator(`[class="folder-name"]`, { hasText: folderName }).click();
    await page.locator('.folder-row:has-text("' + folderName + '") .settings').click();
    await subscriberNavBarPage.clickOnDeleteFolder();
    await expect(page.locator(`[class="folder-name"]`, { hasText: folderName })).not.toBeVisible();
  });
});
