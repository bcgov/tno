const { test, expect } = require('../../../../fixtures/ui-fixture');
const DataLoader = require('../../../../utils/dataLoader');
const CONSTANTS = require('../../../../utils/constants');
const { SubscriberSearchResultPage } = require('../../../../pages/subscriberSearchResultPage');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

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
  subscriberMMIProductPage;

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
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(2000);
});

async function openSubscriberMMIProductsPage() {
  await appPage.navigateToSubscriberURL({ clearCookies: true });
  await appPage.loginAsOtherSubscriber(process.env.SUB_USERNAME1, process.env.SUB_PASSWORD1);
  await subscriberNavBarPage.clickOnLeftNavOptionByText(
    CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
  );
}

async function openEditorMMIProductsPage() {
  await appPage.navigateToUrl(editorUrl);
  await editorHomePage.verifyEditorHomePageLoaded();
  await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
  await appPage.clickOnMenuAndSubNavigationMenuLink(
    CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
  );
}

async function approvePendingProductRequest(productName) {
  await openEditorMMIProductsPage();
  await addProductPage.selectProduct(productName);
  await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
  await addProductPage.approveSubscription();
  await addProductPage.save();
}

async function ensureSubscriberActionIsSubscribe(productName) {
  const currentAction = await subscriberMMIProductPage.getCancelSubscribedText(productName);
  if (currentAction === 'SUBSCRIBE') {
    return;
  }

  if (currentAction !== 'UNSUBSCRIBE') {
    throw new Error(
      `Cannot normalize "${productName}" to SUBSCRIBE. Current action is "${currentAction}".`,
    );
  }

  await subscriberMMIProductPage.clickUnSubscribeButtonForGivenProduct(productName);
  await appPage.logOutFromSubscriber();
  await approvePendingProductRequest(productName);
  await appPage.logOut();
  await openSubscriberMMIProductsPage();

  const normalizedAction = await subscriberMMIProductPage.getCancelSubscribedText(productName);
  if (normalizedAction !== 'SUBSCRIBE') {
    throw new Error(
      `Failed to normalize "${productName}" to SUBSCRIBE. Current action is "${normalizedAction}".`,
    );
  }
}

test.describe('@smoke MMI Product Subscription request functionality', () => {
  test(`Verify edior can Approve MMI Product Subscription Request from Report building MMI Products`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await editorHomePage.addNewProduct();
    expect(await addProductPage.verifyBackToProductsVisibility()).toBe(true);
    const productName = `ProductTitle_${Date.now()}`;
    await addProductPage.enterProductDetails(productName, 'Evening Overview', 'Weekday');
    await reportPage.checkIsPublicCheckbox();
    await addProductPage.saveAndVerifyProductSaved(productName);
    await addProductPage.backToProductGrid();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(true);
    await appPage.logOut();
    await openSubscriberMMIProductsPage();
    await ensureSubscriberActionIsSubscribe(productName);
    await subscriberMMIProductPage.clickSubscribeButtonFOrGivenProduct(productName);
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe(
      'CANCEL PENDING REQUEST',
    );
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    expect(await addProductPage.verifyUserNameVisibility()).toBe(true);
    expect(await addProductPage.verifyApproveButtonVisibility()).toBe(true);
    expect(await addProductPage.verifyRejectButtonVisibility()).toBe(true);
    await addProductPage.approveSubscription();
    await addProductPage.save();
    await appPage.logOut();
    await appPage.navigateToSubscriberURL({ clearCookies: true });
    await appPage.loginAsOtherSubscriber(process.env.SUB_USERNAME1, process.env.SUB_PASSWORD1);
    await subscriberNavBarPage.clickOnLeftNavOptionByText(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe('UNSUBSCRIBE');
    await subscriberMMIProductPage.clickUnSubscribeButtonForGivenProduct(productName);
    await subscriberMMIProductPage.hardWait(2000);
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe(
      'CANCEL PENDING REQUEST',
    );
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    await addProductPage.approveSubscription();
    await addProductPage.save();
    await addProductPage.hardWait(2000);
    await addProductPage.deleteProduct();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(false);
    await appPage.logOut();
  });
  test(`Verify edior can Reject MMI Product Subscription Request from Report building MMI Products`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await editorHomePage.addNewProduct();
    expect(await addProductPage.verifyBackToProductsVisibility()).toBe(true);
    const productName = `ProductTitle_${Date.now()}`;
    await addProductPage.enterProductDetails(productName, 'Evening Overview', 'Weekday');
    await reportPage.checkIsPublicCheckbox();
    await addProductPage.saveAndVerifyProductSaved(productName);
    await addProductPage.backToProductGrid();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(true);
    await appPage.logOut();
    await openSubscriberMMIProductsPage();
    await ensureSubscriberActionIsSubscribe(productName);
    await subscriberMMIProductPage.clickSubscribeButtonFOrGivenProduct(productName);
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe(
      'CANCEL PENDING REQUEST',
    );
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    expect(await addProductPage.verifyUserNameVisibility()).toBe(true);
    expect(await addProductPage.verifyApproveButtonVisibility()).toBe(true);
    expect(await addProductPage.verifyRejectButtonVisibility()).toBe(true);
    await addProductPage.rejectSubscription();
    await addProductPage.save();
    await appPage.logOut();
    await appPage.navigateToSubscriberURL({ clearCookies: true });
    await appPage.loginAsOtherSubscriber(process.env.SUB_USERNAME1, process.env.SUB_PASSWORD1);
    await subscriberNavBarPage.clickOnLeftNavOptionByText(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe('SUBSCRIBE');
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.deleteProduct();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(false);
    await appPage.logOut();
  });
  test(`Verify edior can Approve MMI Product Un-Subscribe Request from Report building MMI Products`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await editorHomePage.addNewProduct();
    expect(await addProductPage.verifyBackToProductsVisibility()).toBe(true);
    const productName = `ProductTitle_${Date.now()}`;
    await addProductPage.enterProductDetails(productName, 'Evening Overview', 'Weekday');
    await reportPage.checkIsPublicCheckbox();
    await addProductPage.saveAndVerifyProductSaved(productName);
    await addProductPage.backToProductGrid();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(true);
    await appPage.logOut();
    await openSubscriberMMIProductsPage();
    await ensureSubscriberActionIsSubscribe(productName);
    await subscriberMMIProductPage.clickSubscribeButtonFOrGivenProduct(productName);
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    await addProductPage.approveSubscription();
    await addProductPage.save();
    await appPage.logOut();
    await appPage.navigateToSubscriberURL({ clearCookies: true });
    await appPage.loginAsOtherSubscriber(process.env.SUB_USERNAME1, process.env.SUB_PASSWORD1);
    await subscriberNavBarPage.clickOnLeftNavOptionByText(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe('UNSUBSCRIBE');
    await subscriberMMIProductPage.clickUnSubscribeButtonForGivenProduct(productName);
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    await addProductPage.approveSubscription();
    await addProductPage.save();
    await appPage.logOut();
    await appPage.navigateToSubscriberURL({ clearCookies: true });
    await appPage.loginAsOtherSubscriber(process.env.SUB_USERNAME1, process.env.SUB_PASSWORD1);
    await subscriberNavBarPage.clickOnLeftNavOptionByText(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe('SUBSCRIBE');
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.deleteProduct();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(false);
    await appPage.logOut();
  });
  test(`Verify edior can Reject MMI Product Un-Subscribe Request from Report building MMI Products`, async ({}) => {
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await editorHomePage.addNewProduct();
    expect(await addProductPage.verifyBackToProductsVisibility()).toBe(true);
    const productName = `ProductTitle_${Date.now()}`;
    await addProductPage.enterProductDetails(productName, 'Evening Overview', 'Weekday');
    await reportPage.checkIsPublicCheckbox();
    await addProductPage.saveAndVerifyProductSaved(productName);
    await addProductPage.backToProductGrid();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(true);
    await appPage.logOut();
    await openSubscriberMMIProductsPage();
    await ensureSubscriberActionIsSubscribe(productName);
    await subscriberMMIProductPage.clickSubscribeButtonFOrGivenProduct(productName);
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    await addProductPage.approveSubscription();
    await addProductPage.save();
    await appPage.logOut();
    await appPage.navigateToSubscriberURL({ clearCookies: true });
    await appPage.loginAsOtherSubscriber(process.env.SUB_USERNAME1, process.env.SUB_PASSWORD1);
    await subscriberNavBarPage.clickOnLeftNavOptionByText(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    expect(await subscriberMMIProductPage.getCancelSubscribedText(productName)).toBe('UNSUBSCRIBE');
    await subscriberMMIProductPage.clickUnSubscribeButtonForGivenProduct(productName);
    await appPage.logOutFromSubscriber();
    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING);
    await appPage.clickOnMenuAndSubNavigationMenuLink(
      CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS,
    );
    await addProductPage.selectProduct(productName);
    await addProductPage.clickOnProductSubTab(CONSTANTS.NAVIGATION_TABS.REQUESTS);
    await addProductPage.rejectSubscription();
    await addProductPage.save();
    await addProductPage.deleteProduct();
    expect(await addProductPage.isAddedProductVisibleOnGrid(productName)).toBe(false);
    await appPage.logOut();
  });
});
