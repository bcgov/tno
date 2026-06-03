const { test, expect } = require('../../../../fixtures/ui-fixture');
const DataLoader = require('../../../../utils/dataLoader');
const CONSTANTS = require('../../../../utils/constants');
const { AddFilterPage } = require('../../../../pages/addFilterPage');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

let page,
  appPage,
  editorHomePage,
  addFoldersPage,
  subscriberSearchResultPage,
  gridPage,
  headlineDetailsPage;

test.beforeEach(async ({ masterFixture }) => {
  page = masterFixture.page;
  appPage = masterFixture.appPage;
  editorHomePage = masterFixture.editorHomePage;
  headlineDetailsPage = masterFixture.headlineDetailsPage;
  subscriberSearchResultPage = masterFixture.subscriberSearchResultPage;
  gridPage = masterFixture.gridPage;
  addFoldersPage = masterFixture.addFoldersPage;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(2000);
});

test.describe('@smoke Verify content page sorting , Use icon fuctionality and edit existing conent', () => {
  test(`Verify sorting on Source columns on content page`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT);
    await editorHomePage.clickOnDateFilterContent(CONSTANTS.BUTTONS.FOURTY_EIGHT_HOURS);
    await editorHomePage.hardWait(1500);
    const rowCount = 500;

    // Check default sorting
    const defaultSourceColumnValuesOnUI = await gridPage.getSourceColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultSourceColumnValuesOnUI, 'ascending')).toBe(
      false,
    );
    expect(await gridPage.isGridColumnSorted(defaultSourceColumnValuesOnUI, 'descending')).toBe(
      false,
    );

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.SOURCE);
    await editorHomePage.selectRecordsOnGrid(rowCount);

    const ascSourceColumnValuesOnUI = await gridPage.getSourceColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(ascSourceColumnValuesOnUI, 'ascending')).toBe(true);

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.SOURCE);
    const descSourceColumnValuesOnUI = await gridPage.getSourceColumnDataFromGrid(rowCount);
    // expect(await gridPage.isGridColumnSorted(descSourceColumnValuesOnUI, 'descending')).toBe(true);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.SOURCE);
    const withoutSortingSourceColumnValuesOnUI =
      await gridPage.getSourceColumnDataFromGrid(rowCount);
    expect(
      await gridPage.isGridColumnSorted(withoutSortingSourceColumnValuesOnUI, 'ascending'),
    ).toBe(false);
    expect(
      await gridPage.isGridColumnSorted(withoutSortingSourceColumnValuesOnUI, 'descending'),
    ).toBe(false);

    await appPage.logOut();
  });

  test(`Verify sorting on Media Type columns on content page`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT);
    await editorHomePage.clickOnDateFilterContent(CONSTANTS.BUTTONS.FOURTY_EIGHT_HOURS);
    await editorHomePage.hardWait(1500);
    const rowCount = 500;
    await editorHomePage.selectRecordsOnGrid(rowCount);

    // Check default sorting
    const defaultMediaTypeColumnValuesOnUI =
      await gridPage.getMediaTypeColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultMediaTypeColumnValuesOnUI, 'ascending')).toBe(
      false,
    );
    expect(await gridPage.isGridColumnSorted(defaultMediaTypeColumnValuesOnUI, 'descending')).toBe(
      false,
    );

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.MEDIA_TYPE);
    const firstMediaTypeSortDirection = await gridPage.getSortingIconState(
      CONSTANTS.COLUMN_NAME.MEDIA_TYPE,
    );
    expect(['ascending', 'descending']).toContain(firstMediaTypeSortDirection);

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.MEDIA_TYPE);
    const secondMediaTypeSortDirection = await gridPage.getSortingIconState(
      CONSTANTS.COLUMN_NAME.MEDIA_TYPE,
    );
    expect(['ascending', 'descending']).toContain(secondMediaTypeSortDirection);
    expect(secondMediaTypeSortDirection).not.toBe(firstMediaTypeSortDirection);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.MEDIA_TYPE);
    const withoutMediaTypeColumnValuesOnUI =
      await gridPage.getMediaTypeColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(withoutMediaTypeColumnValuesOnUI, 'ascending')).toBe(
      false,
    );
    expect(await gridPage.isGridColumnSorted(withoutMediaTypeColumnValuesOnUI, 'descending')).toBe(
      false,
    );

    await appPage.logOut();
  });

  test(`Verify sorting on Published Date columns on content page`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT);
    await editorHomePage.clickOnDateFilterContent(CONSTANTS.BUTTONS.FOURTY_EIGHT_HOURS);
    await editorHomePage.hardWait(1500);
    const rowCount = 500;

    // Check default sorting
    const defaultPublishedDateColumnValuesOnUI =
      await editorHomePage.getPublishedDateFromGrid(rowCount);
    console.log(`Publish date is  : ${defaultPublishedDateColumnValuesOnUI}`);
    expect(
      await gridPage.isDateColumnSorted(defaultPublishedDateColumnValuesOnUI, 'ascending'),
    ).toBe(false);

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.PUB_DATE);
    await editorHomePage.selectRecordsOnGrid(rowCount);

    const ascPublishedDateColumnValuesOnUI =
      await editorHomePage.getPublishedDateFromGrid(rowCount);
    expect(await gridPage.isDateColumnSorted(ascPublishedDateColumnValuesOnUI, 'ascending')).toBe(
      true,
    );

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.PUB_DATE);
    const descPublishedDateColumnValuesOnUI =
      await editorHomePage.getPublishedDateFromGrid(rowCount);
    // expect(await gridPage.isDateColumnSorted(descPublishedDateColumnValuesOnUI, 'descending')).toBe(true);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.PUB_DATE);
    const withoutPublishedDateColumnValuesOnUI =
      await editorHomePage.getPublishedDateFromGrid(rowCount);
    expect(
      await gridPage.isDateColumnSorted(withoutPublishedDateColumnValuesOnUI, 'ascending'),
    ).toBe(false);

    await appPage.logOut();
  });

  test(`Verify Publish and Unpublish functionalty on content page using USE icon`, async ({}) => {
    const parentPage = page;
    const headlineTitle = `Automation Use Icon Headline Title ${Date.now()}`;
    headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.CONTENTS.IMAGE);

    await headlineDetailsPage.enterHeadLineTitle(headlineTitle);
    await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
    await headlineDetailsPage.enterSummary('Automation_Test_Summary');
    await headlineDetailsPage.uploadRadioTVContentFile('News_Article.png');
    await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
    await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
    await headlineDetailsPage.saveHeadlinesWithoutPublish();
    expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);

    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT);
    await expect(page.getByText(headlineTitle).first()).toBeVisible({
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });

    await gridPage.selectUseIconByTitle(headlineTitle);
    expect(await addFoldersPage.isSuccessToastNotificationDisplayed()).toBe(true);

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);

    await subscriberSearchResultPage.clickOnSearchButton();
    await subscriberSearchResultPage.verifySearchResultPageLoaded();

    expect(await subscriberSearchResultPage.isPublishedHeadlinesPresent(headlineTitle)).toBe(true);
    await appPage.logOutFromSubscriber();

    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT);

    headlineDetailsPage = await editorHomePage.clickOnHeadlinesByGivenTitle(headlineTitle);
    await headlineDetailsPage.unPublishHeadlines();
    await headlineDetailsPage.deleteUnpublishedHeadline();
    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await appPage.logOut();
  });

  test(`Verify existing content editing functionality`, async ({}) => {
    const parentPage = page;
    const headlineTitle = `Automation Headline Title ${Date.now()}`;
    headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.CONTENTS.IMAGE);

    await headlineDetailsPage.enterHeadLineTitle(headlineTitle);

    await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
    await headlineDetailsPage.enterSummary('Automation_Test_Summary');
    await headlineDetailsPage.uploadRadioTVContentFile('News_Article.png');

    await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
    await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
    await headlineDetailsPage.saveHeadlinesWithoutPublish();

    expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);
    expect(await headlineDetailsPage.isDeleteButtonVisible()).toBe(true);
    expect(await headlineDetailsPage.isNextPreviewButtonVisible()).toBe(true);

    await headlineDetailsPage.publishHeadlines();
    expect(await headlineDetailsPage.verifyToastNotificationVisible(headlineTitle)).toBe(true);

    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();
    await appPage.logOut();

    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT);

    await editorHomePage.selectMediaTypeFilterDailyPrint(CONSTANTS.HEADLINES.DAILY_PRINT);
    headlineDetailsPage = await editorHomePage.clickOnHeadlinesByGivenTitle(headlineTitle);
    const editedHeadlineTitle = `Edited ${headlineTitle}`;
    await headlineDetailsPage.enterHeadLineTitle(editedHeadlineTitle);
    await headlineDetailsPage.publishHeadlines();
    expect(
      await headlineDetailsPage.verifyToastNotificationVisible(editedHeadlineTitle),
    ).toBe(true);

    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await expect(page.getByText(editedHeadlineTitle).first()).toBeVisible({
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });

    headlineDetailsPage = await editorHomePage.clickOnHeadlinesByGivenTitle(editedHeadlineTitle);
    await headlineDetailsPage.unPublishHeadlines();
    await headlineDetailsPage.deleteUnpublishedHeadline();
    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await appPage.logOut();
  });
});
