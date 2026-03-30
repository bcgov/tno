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

test.describe('@smoke Verify filtering of headlines using Published date content filter', () => {

    test(`Editor filters headlines using Today Published date filter`, async ({}) => {
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME);

      await editorHomePage.clickOnDateFilterContent(CONSTANTS.BUTTONS.TODAY);
      expect(await editorHomePage.isGivenContentSearchFilterPresent(CONSTANTS.BUTTONS.TODAY)).toBe(true);
      const totalRows = await editorHomePage.getTotalHeadlineCount();
      await editorHomePage.selectRecordsOnGrid(totalRows);

      await editorHomePage.validatePublishedDates(CONSTANTS.BUTTONS.TODAY, totalRows);
      await appPage.logOut();
    });

    test(`Editor filters headlines using 24 Hours Published date filter`, async ({}) => {
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME);

      expect(await editorHomePage.isGivenContentSearchFilterPresent(CONSTANTS.BUTTONS.TWENTY_FOUR_HOURS)).toBe(true);
      await editorHomePage.selectMediaTypeFilterDailyPrint(CONSTANTS.HEADLINES.DAILY_PRINT);
      await editorHomePage.clickOnDateFilterContent(CONSTANTS.BUTTONS.TWENTY_FOUR_HOURS);
      await editorHomePage.hardWait(1500);

      const totalRows = await editorHomePage.getTotalHeadlineCount();
      await editorHomePage.selectRecordsOnGrid(totalRows);

      await editorHomePage.validatePublishedDates(CONSTANTS.BUTTONS.TWENTY_FOUR_HOURS, totalRows);
      await appPage.logOut();
    });

    test(`Editor filters headlines using 48 Hours Published date filter`, async ({}) => {
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME);

      expect(await editorHomePage.isGivenContentSearchFilterPresent(CONSTANTS.BUTTONS.FOURTY_EIGHT_HOURS)).toBe(true);
      await editorHomePage.selectMediaTypeFilterDailyPrint(CONSTANTS.HEADLINES.DAILY_PRINT);
      await editorHomePage.clickOnDateFilterContent(CONSTANTS.BUTTONS.FOURTY_EIGHT_HOURS);
      await editorHomePage.hardWait(1500);

      const totalRows = await editorHomePage.getTotalHeadlineCount();
      await editorHomePage.selectRecordsOnGrid(totalRows);

      await editorHomePage.validatePublishedDates(CONSTANTS.BUTTONS.FOURTY_EIGHT_HOURS, totalRows);
      await appPage.logOut();
    });

    test(`Editor filters headlines using Media Type file option Dailyprint`, async ({}) => {
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME);

      expect(await editorHomePage.isGivenContentSearchFilterPresent(CONSTANTS.BUTTONS.FOURTY_EIGHT_HOURS)).toBe(true);
      await editorHomePage.selectMediaTypeFilterDailyPrint(CONSTANTS.HEADLINES.DAILY_PRINT);
      await editorHomePage.hardWait(1500);

      const totalRows = await editorHomePage.getTotalHeadlineCount();
      await editorHomePage.selectRecordsOnGrid(totalRows);
      await editorHomePage.hardWait(2000);

      const mediaTypeRowCount = await editorHomePage.getMediaTypeFromGrid(CONSTANTS.HEADLINES.DAILY_PRINT);
      expect(mediaTypeRowCount).toBe(totalRows);

      await appPage.logOut();
    });

    test(`Editor publishes Print content and verifies it using shown only Print content filter`, async ({ }) => {
      const parentPage = page;
      const headlineTitle = `Automation Print Content Headline Title ${Date.now()}`;
      headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.HEADLINES.PRINT_CONTENT);

      await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
      await headlineDetailsPage.enterSummary('Automation_Test_Summary');

      await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.enterPrepTime('5');

      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToUrl(editorUrl);
      await editorHomePage.verifyEditorHomePageLoaded();
      
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME)
      await editorHomePage.selectShowOnlyFilterChecbox(CONSTANTS.HEADLINES.PRINT_CONTENT);
      expect(await editorHomePage.isPublishedHeadlinesPresent(headlineTitle)).toBe(true);
      headlineDetailsPage = await editorHomePage.clickOnHeadlinesTitleByRowNumber(1);
      await headlineDetailsPage.unPublishHeadlines();

      await headlineDetailsPage.deleteUnpublishedHeadline();
      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

    });

    test(`Editor publishes Print content and verifies it using shown only Published filter`, async ({ }) => {
      const parentPage = page;
      const headlineTitle = `Automation Print Content Headline Title ${Date.now()}`;
      headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.HEADLINES.PRINT_CONTENT);

      await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
      await headlineDetailsPage.enterSummary('Automation_Test_Summary');

      await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.enterPrepTime('5');

      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToUrl(editorUrl);
      await editorHomePage.verifyEditorHomePageLoaded();
      
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME)
      await editorHomePage.selectShowOnlyFilterChecbox(CONSTANTS.HEADLINES.PUBLISHED);
      expect(await editorHomePage.isPublishedHeadlinesPresent(headlineTitle)).toBe(true);
      headlineDetailsPage = await editorHomePage.clickOnHeadlinesTitleByRowNumber(1);
      await headlineDetailsPage.unPublishHeadlines();

      await headlineDetailsPage.deleteUnpublishedHeadline();
      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

    });

    test(`Editor publishes Top Story headline and verifies it using shown only Top Stories filter`, async ({ }) => {
      const parentPage = page;
      const headlineTitle = `Automation Print Content Headline Title ${Date.now()}`;
      headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.HEADLINES.PRINT_CONTENT);

      await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
      await headlineDetailsPage.enterSummary('Automation_Test_Summary');
      await headlineDetailsPage.selectTopStoriesOrCommentaryChecbox(CONSTANTS.HEADLINES.TOP_STORIES);

      await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.enterPrepTime('5');

      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToUrl(editorUrl);
      await editorHomePage.verifyEditorHomePageLoaded();
      
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME)
      await editorHomePage.selectShowOnlyFilterChecbox(CONSTANTS.HEADLINES.TOP_STORIES);
      expect(await editorHomePage.isPublishedHeadlinesPresent(headlineTitle)).toBe(true);
      headlineDetailsPage = await editorHomePage.clickOnHeadlinesTitleByRowNumber(1);
      await headlineDetailsPage.unPublishHeadlines();

      await headlineDetailsPage.deleteUnpublishedHeadline();
      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

    });

    test(`Editor publishes Commentary headline and verifies it using shown only Commentary filter`, async ({ }) => {
      const parentPage = page;
      const headlineTitle = `Automation Print Content Headline Title ${Date.now()}`;
      headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.HEADLINES.PRINT_CONTENT);

      await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
      await headlineDetailsPage.enterSummary('Automation_Test_Summary');
      await headlineDetailsPage.selectTopStoriesOrCommentaryChecbox(CONSTANTS.HEADLINES.COMMENTARY);

      await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.enterPrepTime('5');

      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToUrl(editorUrl);
      await editorHomePage.verifyEditorHomePageLoaded();
      
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME)
      await editorHomePage.selectShowOnlyFilterChecbox(CONSTANTS.HEADLINES.COMMENTARY);
      expect(await editorHomePage.isPublishedHeadlinesPresent(headlineTitle)).toBe(true);
      headlineDetailsPage = await editorHomePage.clickOnHeadlinesTitleByRowNumber(1);
      await headlineDetailsPage.unPublishHeadlines();

      await headlineDetailsPage.deleteUnpublishedHeadline();
      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

    });

    test(`Editor filters headlines using Start date and End Date filter`, async ({}) => {
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME);

      expect(await editorHomePage.isStartDateFilterPresent()).toBe(true);
      expect(await editorHomePage.isEndDateFilterPresent()).toBe(true);
      await editorHomePage.selectMediaTypeFilterDailyPrint(CONSTANTS.HEADLINES.DAILY_PRINT);

      await editorHomePage.selectStartDateAndEndDateFilter(CONSTANTS.FILTER_DATE.START_DATE, CONSTANTS.FILTER_DATE.END_DATE);
      await editorHomePage.hardWait(1500);

      const totalRows = await editorHomePage.getTotalHeadlineCount();
      const publishedDatesOnUI = await editorHomePage.getPublishedDateFromGrid(totalRows);

      expect(await editorHomePage.validateDatesInRange(publishedDatesOnUI, CONSTANTS.FILTER_DATE.START_DATE, CONSTANTS.FILTER_DATE.END_DATE)).toBe(true);

      await appPage.logOut();
    });

     test(`Editor perform search using advance search with Headline title`, async ({ }) => {
      const parentPage = page;
      const headlineTitle = `Automation Print Content Headline Title ${Date.now()}`;
      headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.HEADLINES.PRINT_CONTENT);

      await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

      await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
      await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
      await headlineDetailsPage.enterSummary('Automation_Test_Summary');
      await headlineDetailsPage.selectTopStoriesOrCommentaryChecbox(CONSTANTS.HEADLINES.COMMENTARY);

      await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
      await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
      await headlineDetailsPage.enterPrepTime('5');

      await headlineDetailsPage.publishHeadlines();
      expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);

      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

      await appPage.navigateToUrl(editorUrl);
      await editorHomePage.verifyEditorHomePageLoaded();
      
      await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.HOME);
      await editorHomePage.selectAdvanceSearchTypeAndValueFilter('Headline', headlineTitle);

      expect(await editorHomePage.isPublishedHeadlinesPresent(headlineTitle)).toBe(true);
      headlineDetailsPage = await editorHomePage.clickOnHeadlinesTitleByRowNumber(1);
      await headlineDetailsPage.unPublishHeadlines();

      await headlineDetailsPage.deleteUnpublishedHeadline();
      await headlineDetailsPage.closePage();
      await parentPage.bringToFront();
      await appPage.logOut();

    });

});