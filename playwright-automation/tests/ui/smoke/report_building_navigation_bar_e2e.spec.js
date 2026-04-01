const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');
const { SubscriberSearchResultPage } = require('../../../pages/subscriberSearchResultPage');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const reportData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/reportSharingData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];
const editorReportUrl = testData[testApp]['editor']['reportUrl'];
const recipientEmail = reportData[testApp]['report']['recipient_email'];

let page, appPage, editorHomePage, reportPage, subscriberNavBarPage, subscriberMyReportPage, addProductPage, addFoldersPage, subscriberSearchResultPage;

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
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(2000);
});

test.describe('@smoke Report building end to end workflow', () => {

  test(`Subscriber should able to see the reports created and send by editor under My Reports section`, async ({}) => {
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
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);
    await reportPage.clickOnToastNotificationCloseButton();

    expect(
      await reportPage.isReportSubTabVisible(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE),
    ).toBeTruthy();
    expect(
      await reportPage.isReportSubTabVisible(CONSTANTS.REPORT_SUBNAVIGATION_TABS.SECTIONS),
    ).toBeTruthy();
    expect(
      await reportPage.isReportSubTabVisible(CONSTANTS.REPORT_SUBNAVIGATION_TABS.PREVIEW),
    ).toBeTruthy();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE);
    await reportPage.checkEnableEditCheckbox();
    await reportPage.clickOnUseDefaultTemplateButton();
    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);
    await reportPage.clickOnToastNotificationCloseButton();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.SECTIONS);
    await reportPage.clickOnReportSectionButtonByName(
      CONSTANTS.REPORT_SECTION_BUTTON.TABLE_OF_CONTENTS,
    );
    await reportPage.clickOnReportSectionButtonByName(CONSTANTS.REPORT_SECTION_BUTTON.TEXT);
    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);
    await reportPage.clickOnToastNotificationCloseButton();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.PREVIEW);
    await reportPage.sendTestEmailPreview(recipientEmail);
    await appPage.logOut();

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);

    await subscriberNavBarPage.clickOnMyContentSectionByText(
      CONSTANTS.SUBSCRIBER_NAV_BAR_OPTIONS.MY_REPORTS,
    );

    await subscriberNavBarPage.hardWait(1500);
    expect(await subscriberMyReportPage.isPublishedReportPresent(reportName)).toBe(true);
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
    expect(await reportPage.verifySucessToastNotification(productName)).toBeTruthy();

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

  test(`Verify private report should not be shown on subscriber portal`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);

    await reportPage.verifyReportPageLoaded();
    expect(await reportPage.isAddNewReportButtonVisible()).toBe(true);
    await reportPage.clickOnAddNewReportButton();
    
    const reportName = `ReportTitle_${Date.now()}`;
    const reportDescription = `Report Description ${Date.now()}`;
    await reportPage.addReportName(reportName);
    await reportPage.addReportDescription(reportDescription);
    await reportPage.selectApplyOwnershipToFilters();

    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE);
    expect(await reportPage.isUseDefaultTemplateButtonClickable()).toBe(false);

    await reportPage.checkEnableEditCheckbox();
    expect(await reportPage.isUseDefaultTemplateButtonClickable()).toBe(true);
    await reportPage.clickOnUseDefaultTemplateButton();
    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);
    await reportPage.clickOnToastNotificationCloseButton();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.SECTIONS);
    await reportPage.clickOnReportSectionButtonByName(CONSTANTS.REPORT_SECTION_BUTTON.TABLE_OF_CONTENTS);
    await reportPage.clickOnReportSectionButtonByName(CONSTANTS.REPORT_SECTION_BUTTON.TEXT);
    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);
    await reportPage.clickOnToastNotificationCloseButton();

    await appPage.logOut();

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);

    await subscriberNavBarPage.clickOnMyContentSectionByText(CONSTANTS.SUBSCRIBER_NAV_BAR_OPTIONS.MY_REPORTS,);
    expect(await subscriberMyReportPage.isPublishedReportPresent(reportName)).toBe(false);
    await appPage.logOutFromSubscriber();

    await appPage.page.goto(editorUrl);
    await appPage.login(process.env.app_username, process.env.app_password);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);
    await reportPage.searchAndDeleteReport(reportName);

  });

  test(`Verify public report should be shown on subscriber portal`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);

    await reportPage.verifyReportPageLoaded();
    await reportPage.clickOnAddNewReportButton();
    
    const reportName = `ReportTitle_${Date.now()}`;
    const reportDescription = `Report Description ${Date.now()}`;
    await reportPage.addReportName(reportName);
    await reportPage.addReportDescription(reportDescription);
    await reportPage.selectApplyOwnershipToFilters();
    await reportPage.checkIsPublicCheckbox();

    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE);
    expect(await reportPage.isUseDefaultTemplateButtonClickable()).toBe(false);

    await reportPage.checkEnableEditCheckbox();
    expect(await reportPage.isUseDefaultTemplateButtonClickable()).toBe(true);
    await reportPage.clickOnUseDefaultTemplateButton();
    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);
    await reportPage.clickOnToastNotificationCloseButton();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.SECTIONS);
    await reportPage.clickOnReportSectionButtonByName(CONSTANTS.REPORT_SECTION_BUTTON.TABLE_OF_CONTENTS);
    await reportPage.clickOnReportSectionButtonByName(CONSTANTS.REPORT_SECTION_BUTTON.TEXT);
    await reportPage.clickOnSaveButton();
    expect(await reportPage.verifySucessToastNotification(reportName)).toBe(true);
    await reportPage.clickOnToastNotificationCloseButton();

    await appPage.logOut();

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);

    await subscriberNavBarPage.clickOnMyContentSectionByText(CONSTANTS.SUBSCRIBER_NAV_BAR_OPTIONS.MY_REPORTS);
    await subscriberNavBarPage.hardWait(1500);
    expect(await subscriberMyReportPage.isPublishedReportPresent(reportName)).toBe(true);
    await appPage.logOutFromSubscriber();

    await appPage.page.goto(editorUrl);
    await appPage.login(process.env.app_username, process.env.app_password);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);
    await reportPage.searchAndDeleteReport(reportName);

  });

  /** Enable Edit template changes are not saved..hence failing  */
  test(`Verify editing the existing Report and persistence across session`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);

    await reportPage.verifyReportPageLoaded();
    await reportPage.clickOnAddNewReportButton();
    
    const reportName = `ReportTitle_${Date.now()}`;
    let reportDescription = `Report Description ${Date.now()}`;
    await reportPage.addReportName(reportName);
    await reportPage.addReportDescription(reportDescription);
    await reportPage.selectApplyOwnershipToFilters();
    await reportPage.checkIsPublicCheckbox();

    await reportPage.clickOnSaveButton();

    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);
    await reportPage.searchAndSelectReport(reportName);

    reportDescription = `Report Description updated ${Date.now()}`;
    await reportPage.addReportDescription(reportDescription);
    await reportPage.checkIsSortOrderIsEnabledCheckbox();

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE);
    await reportPage.checkEnableEditCheckbox();
    await reportPage.clickOnUseDefaultTemplateButton();
    await reportPage.clickOnSaveButton();

    await appPage.logOut();

    await appPage.page.goto(editorUrl);
    await appPage.login(process.env.app_username, process.env.app_password);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);  
    await reportPage.searchAndSelectReport(reportName);

    expect(await reportPage.getReportDescription()).toBe(reportDescription);
    expect(await reportPage.isSortOrderEnabledCheckboxSelected()).toBe(true);

    await reportPage.clickOnReportSubTab(CONSTANTS.REPORT_SUBNAVIGATION_TABS.TEMPLATE);
    expect(await reportPage.isUseDefaultTemplateButtonClickable()).toBe(true);

    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);
    await reportPage.searchAndDeleteReport(reportName);

    await appPage.logOut();

  });

  test(`Verify Add new Report field validation`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS);

    await reportPage.verifyReportPageLoaded();
    await reportPage.clickOnAddNewReportButton();

    const reportDescription = `Report Description ${Date.now()}`;
    await reportPage.addReportDescription(reportDescription);
    await reportPage.selectApplyOwnershipToFilters();
    await reportPage.checkIsPublicCheckbox();

    await reportPage.clickOnSaveButton();
    expect(await reportPage.getToastValidationMessage()).toBe('Please refer to the highlighted tab and fix the validation errors.');

    await reportPage.enterSortOrder('1');
    await reportPage.clickOnSaveButton();
    expect(await reportPage.getNameFieldValidationMessageLable()).toBe('Report should have a name.');
    
    await appPage.logOut();

  });

  test(`Validate Report page access using Subscriber login`, async ({}) => {
    await appPage.logOut();

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsOtherSubscriber(process.env.sub_username1, process.env.sub_password1);

    await appPage.page.goto(editorReportUrl);
    expect(await subscriberSearchResultPage.isEditorPageDisplayed()).toBe(false);
    await appPage.logOut();
    
  });

  test(`Verify edior can add new Folder from Report building Folders menu`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.FOLDERS);

    await editorHomePage.addNewFolder();
    expect(await addFoldersPage.verifyBackToFolderssVisibility()).toBe(true)

    const folderName = `FoldersTitle_${Date.now()}`;
    await addFoldersPage.enterFoldersDetails(folderName, 'Test Automation Description');
    await addFoldersPage.save();
    expect(await reportPage.verifySucessToastNotification(folderName)).toBe(true);

    await addFoldersPage.clickOnFoldersSubTab(CONSTANTS.NAVIGATION_TABS.COLLECTION);
    await addFoldersPage.selectFilter(CONSTANTS.HEADLINES.TOP_STORIES)
    await addFoldersPage.save();

    await addFoldersPage.clickOnFoldersSubTab(CONSTANTS.NAVIGATION_TABS.CONTENT);
    await addFoldersPage.save();

    await addFoldersPage.backToFoldersGrid();
    expect(await addFoldersPage.isAddedFolderVisibleOnGrid(folderName)).toBe(true);

    await addFoldersPage.selectFolder(folderName);
    await addFoldersPage.deleteProduct();

     expect(await addFoldersPage.isAddedFolderVisibleOnGrid(folderName)).toBe(false);
     await appPage.logOut();
  });

  test(`Verify Add new Folder field validation`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.REPORTBUILDING_SUBMENU.FOLDERS);

     await editorHomePage.addNewFolder();

    const folderDescription = `Folder Description ${Date.now()}`;
    await addFoldersPage.addFolderDescription(folderDescription);

    await addFoldersPage.save();
    expect(await addFoldersPage.isSuccessToastNotificationDisplayed()).toBe(false);

    await reportPage.enterSortOrder('2');
    await addFoldersPage.save();
    expect(await addFoldersPage.isSuccessToastNotificationDisplayed()).toBe(false);
    
    await appPage.logOut();

  });


});
