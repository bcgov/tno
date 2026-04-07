const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');
const { describe } = require('node:test');

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

test.describe('@smoke Delete Un-Publishing headline workflow', () => {

    test(`Delete Un-Publish the headline published by Editor`, async ({ }) => {
      const parentPage = page;
      const headlineTitle = `Automation Headline Title ${Date.now()}`;
      headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.CONTENTS.IMAGE);

      await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

      await headlineDetailsPage.selectMediaOutlet(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
      await headlineDetailsPage.enterSummary('Automation_Test_Summary');
      await headlineDetailsPage.uploadRadioTVContentFile('News_Article.png');

      await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.publishHeadlines();

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToSubscriberURL();
      await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);
      
      await subscriberSearchResultPage.clickOnSearchButton();
      await subscriberSearchResultPage.verifySearchResultPageLoaded();

      expect(await subscriberSearchResultPage.isPublishedHeadlinesPresent(headlineTitle)).toBeTruthy();
      await appPage.logOutFromSubscriber();

      await appPage.navigateToUrl(editorUrl);
      await editorHomePage.verifyEditorHomePageLoaded();
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT);

      await editorHomePage.selectMediaTypeFilterDailyPrint(CONSTANTS.HEADLINES.DAILY_PRINT);
      expect(await editorHomePage.isHeadLineTitletPresentOnEditorGrid(headlineTitle)).toBeTruthy();
      headlineDetailsPage = await editorHomePage.clickOnHeadlinesTitleByRowNumber(1);
      await headlineDetailsPage.unPublishHeadlines();

      await headlineDetailsPage.deleteUnpublishedHeadline();
      await editorHomePage.selectMediaTypeFilterDailyPrint(CONSTANTS.HEADLINES.DAILY_PRINT);
      expect(await editorHomePage.isHeadLineTitletPresentOnEditorGrid(headlineTitle)).toBeFalsy();
      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToSubscriberURL();
      await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);
      
      await subscriberSearchResultPage.clickOnSearchButton();
      await subscriberSearchResultPage.verifySearchResultPageLoaded();

      expect(await subscriberSearchResultPage.isPublishedHeadlinesPresent(headlineTitle)).toBeFalsy();
      await appPage.logOutFromSubscriber();
      
    });

});
