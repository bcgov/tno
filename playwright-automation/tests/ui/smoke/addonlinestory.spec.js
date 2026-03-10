const { test, expect } = require('../../../fixtures/ui-fixture');
const AppPage = require('../../../pages/appPage');
const env = require('../../../config/env.config');
const DataLoader = require('../../../utils/dataLoader');

const users = DataLoader.loadJSON('test-data/loginData.json');

test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test.describe('@smoke Adding an online story by editor and publishing it ', () => {
  test(`Login as ${env.username}`, async ({ page }) => {
    const appPage = new AppPage(page);
    await appPage.mmiMSLogin(env.username, env.password);
    await expect(page).toHaveURL('/contents');
    await appPage.contentCreationOnline();
    await appPage.selectFeaturedStories();
    await appPage.selectTopStories();
    await appPage.selectCommentary();
    const randomNum = Math.floor(Math.random() * 10000);
    const headline = `Test Online Story Headline - ${randomNum}`;
    await appPage.enterHeadline(headline);
    await appPage.selectTNOOption();
    await appPage.enterParagraph(`This is a test paragraph for the online story created by automation script. Random number: ${randomNum}`);
    await appPage.selectPointsAndClickPublish();
    console.log(`Published story with headline: ${headline}`);
    const toastMessage = await appPage.getToastMessage();
    await expect(toastMessage).toContain('has successfully been saved.');
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await appPage.navigatefeaturedStories();
    const verifiedHeadline = await appPage.verifyOnlineStory(randomNum);
    console.log(`Verified headline: ${verifiedHeadline}`);
    console.log(`Expected headline: ${headline}`);
    expect(verifiedHeadline).toBe(headline);
  });


});
