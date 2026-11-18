const { test, expect } = require('../../../../fixtures/ui-fixture');
const DataLoader = require('../../../../utils/dataLoader');
const CONSTANTS = require('../../../../utils/constants');
const { SubscriberSearchResultPage } = require('../../../../pages/subscriberSearchResultPage');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const reportData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/reportSharingData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];
const editorReportUrl = testData[testApp]['editor']['reportUrl'];
const recipientEmail = reportData[testApp]['report']['recipient_email'];

let page,
  appPage,
  editorHomePage,
  reportPage,
  subscriberNavBarPage,
  subscriberMyReportPage,
  addProductPage,
  addFoldersPage,
  subscriberSearchResultPage,
  addFilterPage,
  editTopicsPage,
  subscriberMMIProductPage,
  gridPage;

test.beforeEach(async ({ masterFixture }) => {
  page = masterFixture.page;
  appPage = masterFixture.appPage;
  editorHomePage = masterFixture.editorHomePage;
  reportPage = masterFixture.reportPage;
  subscriberNavBarPage = masterFixture.subscriberNavBarPage;
  subscriberNavBarPage = masterFixture.subscriberNavBarPage;
  subscriberMyReportPage = masterFixture.subscriberMyReportPage;
  addProductPage = masterFixture.addProductPage;
  addFoldersPage = masterFixture.addFoldersPage;
  subscriberSearchResultPage = masterFixture.subscriberSearchResultPage;
  addFilterPage = masterFixture.addFilterPage;
  editTopicsPage = masterFixture.editTopicsPage;
  subscriberMMIProductPage = masterFixture.subscriberMMIProductPage;
  gridPage = masterFixture.gridPage;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(2000);
});

test.describe('@smoke Filters page grid column Sorting functionality', () => {
  test(`Verify sorting on Name columns on Filter page grid`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.FILTERS);
    await editorHomePage.hardWait(1500);
    const rowCount = 500;

    // Check default sorting
    const defaultNameColumnValuesOnUI =
      await gridPage.getFiltersGridNameColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultNameColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(defaultNameColumnValuesOnUI, 'descending')).toBe(
      false,
    );

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.NAME);
    expect(await gridPage.getSortingIconState(CONSTANTS.COLUMN_NAME.NAME)).toBe('ascending');

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.NAME);
    expect(await gridPage.getSortingIconState(CONSTANTS.COLUMN_NAME.NAME)).toBe('descending');

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.NAME);

    await appPage.logOut();
  });

  test(`Verify sorting on Owner columns on Report page grid`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.FILTERS);
    await editorHomePage.hardWait(1500);
    const rowCount = 500;

    // Check default sorting
    const defaultOwnerColumnValuesOnUI =
      await gridPage.getFiltersGridOwnerColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultOwnerColumnValuesOnUI, 'ascending')).toBe(
      false,
    );
    expect(await gridPage.isGridColumnSorted(defaultOwnerColumnValuesOnUI, 'descending')).toBe(
      false,
    );

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.OWNER);
    // await editorHomePage.selectRecordsOnGrid(rowCount);

    const ascOwnerColumnValuesOnUI = await gridPage.getFiltersGridOwnerColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(ascOwnerColumnValuesOnUI, 'ascending')).toBe(true);

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.OWNER);
    const descOwnerColumnValuesOnUI =
      await gridPage.getFiltersGridOwnerColumnDataFromGrid(rowCount);
    // expect(await gridPage.isGridColumnSorted(descOwnerColumnValuesOnUI, 'descending')).toBe(true);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.OWNER);
    const withoutSortingOwnerColumnValuesOnUI =
      await gridPage.getFiltersGridOwnerColumnDataFromGrid(rowCount);
    expect(
      await gridPage.isGridColumnSorted(withoutSortingOwnerColumnValuesOnUI, 'ascending'),
    ).toBe(false);
    expect(
      await gridPage.isGridColumnSorted(withoutSortingOwnerColumnValuesOnUI, 'descending'),
    ).toBe(false);

    await appPage.logOut();
  });

  test(`Verify sorting on Description columns on MMI Product page`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.FILTERS);
    await editorHomePage.hardWait(1500);
    const rowCount = 500;

    // Check default sorting
    const defaultDescriptionColumnValuesOnUI =
      await gridPage.getFiltersGridDescriptionColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultDescriptionColumnValuesOnUI, 'ascending')).toBe(
      false,
    );
    expect(
      await gridPage.isGridColumnSorted(defaultDescriptionColumnValuesOnUI, 'descending'),
    ).toBe(false);

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.DESCRIPTION);
    expect(await gridPage.getSortingIconState(CONSTANTS.COLUMN_NAME.DESCRIPTION)).toBe(
      'ascending',
    );

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.DESCRIPTION);
    expect(await gridPage.getSortingIconState(CONSTANTS.COLUMN_NAME.DESCRIPTION)).toBe(
      'descending',
    );

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.DESCRIPTION);

    await appPage.logOut();
  });
});
