const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, editorOnlineStoryPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  editorOnlineStoryPage = masterFixture.editorOnlineStoryPage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Adding a new minister', () => {
  test(`Adding a new minister`, async ({  }) => {
    await expect(page).toHaveURL(mmiMSUrl+'contents');
    await editorOnlineStoryPage.navigateToContentConfiguration();
    await editorOnlineStoryPage.navigateToMinisters();
    await editorOnlineStoryPage.clickAddNewMinister();
    const randomNum = Math.floor(Math.random() * 10000);
    const ministerName = `Test Minister Name - ${randomNum}`;
    const ministerDescription = `Test Minister Description - ${randomNum}`;
    const ministerPosition = `Test Minister Position - ${randomNum}`;
    const ministerSortOrder = 1;

    await editorOnlineStoryPage.enterMinisterDetails(
      ministerName,
      ministerDescription,
      ministerPosition,
      ministerSortOrder
    );

    await page.waitForTimeout(10000);
    await editorOnlineStoryPage.clickBackToMinisters();
    await page.waitForTimeout(2000);
    await editorOnlineStoryPage.searchMinisterByName(ministerName);
    const searchedMinisterName = await editorOnlineStoryPage.getSearchedMinisterName(ministerName);
    console.log(`Searched Minister Name: ${searchedMinisterName}`);
    console.log(`Expected Minister Name: ${ministerName}`);
    await expect(searchedMinisterName).toContain(ministerName);
  });
});