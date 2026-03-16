const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const reportData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/reportSharingData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];
const recipientEmail = reportData[testApp]['report']['recipient_email'];

let page, appPage, editorHomePage, reportPage, subscriberNavBarPage, subscriberMyReportPage, addProductPage;

test.beforeEach(async ({ masterFixture }) => {
  page = masterFixture.page;
  appPage = masterFixture.appPage;
  editorHomePage = masterFixture.editorHomePage;
  reportPage = masterFixture.reportPage;
  subscriberNavBarPage = masterFixture.subscriberNavBarPage;
  subscriberNavBarPage = masterFixture.subscriberNavBarPage;
  subscriberMyReportPage = masterFixture.subscriberMyReportPage;
  addProductPage = masterFixture.addProductPage;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(2000);
});

test.describe('@smoke Report building end to end workflow', () => {

  test(`Subscriber should able to see the reports creted and send by editor under My Reports section`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);

    await reportPage.verifyReportPageLoaded();
    await reportPage.clickOnAddNewReportButton();
    expect( await reportPage.isBackToReportButtonVisible()).toBeTruthy();
    expect(
      await reportPage.isReportSubTabVisible(CONSTANTS.REPORT_SUBNAVIGATION_TABS.REPORT),
    ).toBeTruthy();
    
    const reportName = `ReportTitle_${Date.now()}`;
    const reportDescription = `Report Description ${Date.now()}`;
    await reportPage.addReportName(reportName);
    await reportPage.addReportDescription(reportDescription);
    await reportPage.selectApplyOwnershipToFilters();
    await reportPage.clickOnSaveButton();
    expect(reportPage.verifySucessToastNotification(reportName)).toBeTruthy();

    expect(
      reportPage.isReportSubTabVisible(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE),
    ).toBeTruthy();
    expect(
      reportPage.isReportSubTabVisible(CONSTANTS.REPORT_SUBNAVIGATION_TABS.SECTIONS),
    ).toBeTruthy();
    expect(
      reportPage.isReportSubTabVisible(CONSTANTS.REPORT_SUBNAVIGATION_TABS.PREVIEW),
    ).toBeTruthy();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE);
    await reportPage.checkEnableEditCheckbox();
    await reportPage.clickOnUseDefaultTemplateButton();
    await reportPage.clickOnSaveButton();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.SECTIONS);
    await reportPage.clickOnReportSectionButtonByName(
      CONSTANTS.REPORT_SECTION_BUTTON.TABLE_OF_CONTENTS,
    );
    await reportPage.clickOnReportSectionButtonByName(CONSTANTS.REPORT_SECTION_BUTTON.TEXT);
    await reportPage.clickOnSaveButton();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.PREVIEW);
    await reportPage.sendTestEmailPreview(recipientEmail);
    await appPage.logOut();

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);

    await subscriberNavBarPage.clickOnMyContentSectionByText(
      CONSTANTS.SUBSCRIBER_NAV_BAR_OPTIONS.MY_REPORTS,
    );
    expect(subscriberMyReportPage.isPublishedReportPresent(reportName)).toBeTruthy();
    await appPage.logOutFromSubscriber();

    await appPage.page.goto(editorUrl);
    await appPage.login(process.env.app_username, process.env.app_password);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);
    await reportPage.searchAndDeleteReport(reportName);

  });

  test(`Verify edior can add new product from Report building MMI Products`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS);

    await editorHomePage.addNewProduct();
    expect(await addProductPage.verifyBackToProductsVisibility()).toBe(true)

    const productName = `ProductTitle_${Date.now()}`;
    await addProductPage.enterProductDetails(productName, 'Evening Overview', 'Weekday');
    await addProductPage.save();
    expect(reportPage.verifySucessToastNotification(productName)).toBeTruthy();

    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.SUBSCRIBERS);
    await addProductPage.selectSubscriber('dJani');
    await addProductPage.save();

    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    await addProductPage.save();

    await addProductPage.backToProductGrid();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(true);

    await addProductPage.selectProduct(productName);
    await addProductPage.deleteProduct();

     expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(false);
     await appPage.logOut();
  });


});
