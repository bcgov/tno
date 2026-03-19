const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

console.log("Editor URL:",editorUrl );

let page, appPage, editorHomePage, headlineDetailsPage, printContentPage, subscriberSearchResultPage;

test.beforeEach(async ({ masterFixture }) => {
    page = masterFixture.page;  
    appPage = masterFixture.appPage;
    editorHomePage = masterFixture.editorHomePage;
    headlineDetailsPage = masterFixture.headlinesDetailsPage;
    subscriberSearchResultPage = masterFixture.subscriberSearchResultPage;
    printContentPage = masterFixture.printContentPage;
    await appPage.navigateToUrl(editorUrl);
    await appPage.hardWait(5000);
});

test.describe('@smoke Publish print Content', () => {
     test(`Login as ${process.env.app_username}`, async ({page}) => {
      console.log("Editor URL:",editorUrl );
      await page.goto(editorUrl);
     });
     test(`Editor Print Content verifies it in portal`, async ({page }) => {
      const parentPage = page;
      await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
      await headlineDetailsPage.enterSummary('Automation_Test_Summary');
      
      await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.enterPrepTime('5');
      await headlineDetailsPage.saveHeadlinesWithoutPublish();
      
      expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBeTruthy();
      expect(await headlineDetailsPage.isDeleteButtonVisible()).toBeTruthy();
      expect(await headlineDetailsPage.isNextPreviewButtonVisible()).toBeTruthy();

      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBeTruthy();

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToSubscriberURL();
      await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);
      
      await subscriberSearchResultPage.clickOnSearchButton();
      await subscriberSearchResultPage.verifySearchResultPageLoaded();

      expect(subscriberSearchResultPage.isPublishedHeadlinesPresent(headlineTitle)).toBeTruthy();
      await appPage.logOutFromSubscriber();

    });

});  
