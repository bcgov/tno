const { test, expect } = require('../../../fixtures/ui-fixture');
const AppPage = require('../../../pages/appPage');
const EditorHomePage = require('../../../pages/editorHomePage')
const HeadlinesDetailsPage = require('../../../pages/headlinesDetailsPage')
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');
const SubscriberSearchResultPage = require('../../../pages/subscriberSearchResultPage');
const env = require('../../../config/env.config');

const users = DataLoader.loadJSON('test-data/loginData.json');


test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test.describe('@smoke Featued content Publishing', () => {

    test(`Editor publishes an item and Subscriber can see it under Featued column`, async ({ page }) => {
       const appPage = new AppPage(page);
       await appPage.login(env.username, env.password);
      
      const editorHomePage = new EditorHomePage(page);
      await editorHomePage.verifyEditorHomePageLoaded();
      const newTab = await editorHomePage.clickOnHeadlinesTitleByRowNumber(1);

      const headlineDetailsPage = new HeadlinesDetailsPage(newTab);
      await headlineDetailsPage.verifyHeadlinesDetailsPageLoaded();
      const editorHeadlineTitle = await headlineDetailsPage.getHeadlinesTextFieldValue();

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(editorHeadlineTitle)).toBeTruthy();

      await newTab.close();
      await page.bringToFront();
      await appPage.logOut();

      await appPage.navigateToSubscriberURL();
      await appPage.loginAsSubscriber(env.sub_username, env.sub_password);
      const subscriberSearchResultPage = new SubscriberSearchResultPage(page);
      await subscriberSearchResultPage.clickOnSearchButton();

      await subscriberSearchResultPage.verifySearchResultPageLoaded();
      expect(subscriberSearchResultPage.isPublishedHeadlinesPresent(editorHeadlineTitle)).toBeTruthy();

    });

});