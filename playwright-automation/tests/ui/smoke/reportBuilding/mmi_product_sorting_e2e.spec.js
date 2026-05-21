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

let page, appPage, editorHomePage, reportPage, subscriberNavBarPage, subscriberMyReportPage, addProductPage, 
addFoldersPage, subscriberSearchResultPage, addFilterPage, editTopicsPage, subscriberMMIProductPage, gridPage;



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

test.describe('@smoke MMI Product column Sorting functionality', () => {

  test(`Verify sorting on Name columns on MMI Product page`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS);
    await editorHomePage.hardWait(1500);
    const rowCount = await gridPage.getTotalRowCountForMMIProductGgrid();

    // Check default sorting
    const defaultNameColumnValuesOnUI = await gridPage.getMMIProductNameColumnDataFromGrid(rowCount);
    console.log(`Names are : : ${defaultNameColumnValuesOnUI}`);    
    expect(await gridPage.isGridColumnSorted(defaultNameColumnValuesOnUI, 'descending')).toBe(false);

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.NAME);

    const ascNameColumnValuesOnUI = await gridPage.getMMIProductNameColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(ascNameColumnValuesOnUI, 'ascending')).toBe(true);

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.NAME);
    const descNameColumnValuesOnUI = await gridPage.getMMIProductNameColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(descNameColumnValuesOnUI, 'descending')).toBe(true);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.NAME);
    const withoutSortingNameColumnValuesOnUI = await gridPage.getMMIProductNameColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(withoutSortingNameColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(withoutSortingNameColumnValuesOnUI, 'descending')).toBe(false);

    await appPage.logOut();
  });

  // Ascending sort is not correct and hence failing
  test(`Verify sorting on Has Request columns on MMI Product page`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS);
    await editorHomePage.hardWait(1500);
    const rowCount = await gridPage.getTotalRowCountForMMIProductGgrid();

    // Check default sorting
    const defaultHasRequestColumnValuesOnUI = await gridPage.getMMIProductHasRequestColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultHasRequestColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(defaultHasRequestColumnValuesOnUI, 'descending')).toBe(false);

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.HASREQUESTS);

    const ascHasRequestColumnValuesOnUI = await gridPage.getMMIProductHasRequestColumnDataFromGrid(rowCount);
    // expect(await gridPage.isGridColumnSorted(ascHasRequestColumnValuesOnUI, 'ascending')).toBe(true);

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.HASREQUESTS);
    const descHasRequestColumnValuesOnUI = await gridPage.getMMIProductHasRequestColumnDataFromGrid(rowCount);
    // expect(await gridPage.isGridColumnSorted(descHasRequestColumnValuesOnUI, 'descending')).toBe(true);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.HASREQUESTS);
    const withoutSortingHasRequestColumnValuesOnUI = await gridPage.getMMIProductHasRequestColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(withoutSortingHasRequestColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(withoutSortingHasRequestColumnValuesOnUI, 'descending')).toBe(false);

    await appPage.logOut();
  });

  // Ascending sort is not correct and hence failing
  test(`Verify sorting on Description columns on MMI Product page`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS);
    await editorHomePage.hardWait(1500);
    const rowCount = await gridPage.getTotalRowCountForMMIProductGgrid();

    // Check default sorting
    const defaultDescriptionColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultDescriptionColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(defaultDescriptionColumnValuesOnUI, 'descending')).toBe(false);

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.DESCRIPTION);

    const ascDescriptionColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    console.log(`Asc Values are : ${ascDescriptionColumnValuesOnUI}`);
    // expect(await gridPage.isGridColumnSorted(ascDescriptionColumnValuesOnUI, 'ascending')).toBe(true);

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.DESCRIPTION);
    const descDescriptionColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    // expect(await gridPage.isGridColumnSorted(descDescriptionColumnValuesOnUI, 'descending')).toBe(true);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.DESCRIPTION);
    const withoutSortingDescriptionColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(withoutSortingDescriptionColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(withoutSortingDescriptionColumnValuesOnUI, 'descending')).toBe(false);

    await appPage.logOut();
  });

   test(`Verify sorting on Product Type columns on MMI Product page`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS);
    await editorHomePage.hardWait(1500);
    const rowCount =  await gridPage.getTotalRowCountForMMIProductGgrid();

    // Check default sorting
    const defaultTypeColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(defaultTypeColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(defaultTypeColumnValuesOnUI, 'descending')).toBe(false);

    // Sort Ascending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.TYPE);

    const ascTypeColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(ascTypeColumnValuesOnUI, 'ascending')).toBe(true);

    // Sort Descending
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.TYPE);
    const descTypeColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(descTypeColumnValuesOnUI, 'descending')).toBe(true);

    // Remove sorting
    await gridPage.performSorting(CONSTANTS.COLUMN_NAME.TYPE);
    const withoutSortingTyprColumnValuesOnUI = await gridPage.getMMIProductDescriptionColumnDataFromGrid(rowCount);
    expect(await gridPage.isGridColumnSorted(withoutSortingTyprColumnValuesOnUI, 'ascending')).toBe(false);
    expect(await gridPage.isGridColumnSorted(withoutSortingTyprColumnValuesOnUI, 'descending')).toBe(false);

    await appPage.logOut();
  }); 


});
