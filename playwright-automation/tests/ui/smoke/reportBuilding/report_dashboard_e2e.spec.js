const { test, expect } = require('../../../../fixtures/ui-fixture');
const CONSTANTS = require('../../../../utils/constants');
const DataLoader = require('../../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);

const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

let page, appPage, editorHomePage, reportPage, subscriberNavBarPage, subscriberMyReportPage, 
addProductPage, addFoldersPage, subscriberSearchResultPage, addFilterPage, reportDashboardPage;



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
  reportDashboardPage = masterFixture.reportDashboardPage;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(2000);
  
});

test.describe('@smoke Report dashborad end to end workflow', () => {

  test(`Verify report dashboard end to end functionality`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.DASHBOARD);

    expect(await reportDashboardPage.getReportDashBoardTitle()).toBe('REPORT DASHBOARD');
    expect(await reportDashboardPage.isFailedStatusVisible()).toBe(true);
    expect(await reportDashboardPage.getTotalRecordsOnGrid()).toBe(1);

    await reportDashboardPage.selectOrDeselectShowFailedOnlyCheckbox();
    expect(await reportDashboardPage.getTotalRecordsOnGrid()).toBeGreaterThan(1);
    expect(await reportDashboardPage.isFailedStatusVisible()).toBe(true);

    await reportDashboardPage.selectOrDeselectShowFailedOnlyCheckbox();
    expect(await reportDashboardPage.isExpandButtonVisible()).toBe(true);
    await reportDashboardPage.clickExpandButtonUnderNextRunForFailedRecord();
    expect(await reportDashboardPage.isChesResponseExpandButtonVisible()).toBe(true);

    await reportDashboardPage.clickChesResponseExpandButtonUnderNextRunForFailedRecord();
    expect(await reportDashboardPage.isErrorTextAreaVisible()).toBe(true);

    await appPage.logOut();

  });

});
