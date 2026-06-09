const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];
const editorUrl = testData[testApp]['editor']['url'];

let page, appPage, editorOnlineStoryPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  editorOnlineStoryPage = masterFixture.editorOnlineStoryPage;
  await appPage.navigateToUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Adding a new minister', () => {
  test(`Adding a new minister`, async ({}) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
    await editorOnlineStoryPage.navigateToContentConfiguration();
    await editorOnlineStoryPage.navigateToMinisters();
    await editorOnlineStoryPage.clickAddNewMinister();
    const ministerName = 'Automation_Minister';
    const ministerDescription = 'Automation_Minister_Description';
    const ministerPosition = 'Automation_Minister_Position';
    const ministerSortOrder = 1;

    await editorOnlineStoryPage.enterMinisterDetails(
      ministerName,
      ministerDescription,
      ministerPosition,
      ministerSortOrder,
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
