const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

let page, appPage, editorHomePage, headlineDetailsPage, subscriberSearchResultPage;


test.beforeEach(async ({ masterFixture }) => {
    page = masterFixture.page;  
    appPage = masterFixture.appPage;
    editorHomePage = masterFixture.editorHomePage;
    headlineDetailsPage = masterFixture.headlineDetailsPage;
    subscriberSearchResultPage = masterFixture.subscriberSearchResultPage;
    await appPage.navigateToUrl(editorUrl);
    await appPage.hardWait(5000);
});

test.describe('@smoke Featued content Publishing', () => {

    test(`Editor publishes an item and Subscriber can see it under Featued column`, async ({ }) => {
      const parentPage = page;
      headlineDetailsPage = await editorHomePage.clickOnHeadlinesTitleByRowNumber(6);

      const editorHeadlineTitle = await headlineDetailsPage.getHeadlinesTextFieldValue();

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(editorHeadlineTitle)).toBeTruthy();

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToSubscriberURL();
      await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);
      
      await subscriberSearchResultPage.clickOnSearchButton();

      await subscriberSearchResultPage.verifySearchResultPageLoaded();
      expect(await subscriberSearchResultPage.isPublishedHeadlinesPresent(editorHeadlineTitle)).toBeTruthy();
      await appPage.logOutFromSubscriber();

    });

});