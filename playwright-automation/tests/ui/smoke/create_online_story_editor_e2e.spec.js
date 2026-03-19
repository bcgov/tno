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

test.describe('@smoke Adding an online story by editor and publishing it ', () => {
  test(`Adding an online story by editor and publishing it`, async ({  }) => {
    await expect(page).toHaveURL(mmiMSUrl+'contents');
    await editorOnlineStoryPage.contentCreationOnline(mmiMSUrl);
    await editorOnlineStoryPage.selectFeaturedStories();
    await editorOnlineStoryPage.selectTopStories();
    await editorOnlineStoryPage.selectCommentary();
    const randomNum = Math.floor(Math.random() * 10000);
    const headline = `Test Online Story Headline - ${randomNum}`;
    await editorOnlineStoryPage.enterHeadline(headline);
    await editorOnlineStoryPage.selectTNOOption();
    await editorOnlineStoryPage.enterParagraph(`This is a test paragraph for the online story created by automation script. Random number: ${randomNum}`);
    await editorOnlineStoryPage.selectPointsAndClickPublish();
    console.log(`Published story with headline: ${headline}`);
    const toastMessage = await editorOnlineStoryPage.getToastMessage();
    await expect(toastMessage).toContain('has successfully been saved.');
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await editorOnlineStoryPage.navigatefeaturedStories();
    const verifiedHeadline = await editorOnlineStoryPage.verifyOnlineStory(randomNum);
    console.log(`Verified headline: ${verifiedHeadline}`);
    console.log(`Expected headline: ${headline}`);
    expect(verifiedHeadline).toBe(headline);
  });


});
