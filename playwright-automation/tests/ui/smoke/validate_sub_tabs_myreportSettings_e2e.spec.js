const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, reportSubscriberSidePage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  reportSubscriberSidePage = masterFixture.reportSubscriberSidePage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Validate My Report Settings Sub Tabs from Subscriber Side', () => {
  test(`Validate My Report Settings Sub Tabs from Subscriber Side`, async ({}) => {
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await reportSubscriberSidePage.clickOnMyreportsLink();
    expect(await reportSubscriberSidePage.newReportButton).toBeVisible();
    await reportSubscriberSidePage.clickOnNewReportButton();
    expect(await reportSubscriberSidePage.reportNameInput).toBeVisible();
    await page.waitForTimeout(1000);
    expect(await reportSubscriberSidePage.settingsTab).toBeVisible();
    await reportSubscriberSidePage.clickOnSettingsTab();
    await page.waitForTimeout(1000);
    expect(await reportSubscriberSidePage.templateTab).toBeVisible();
    await reportSubscriberSidePage.clickOnTemplateTab();
    expect(await reportSubscriberSidePage.dataSourcesTab).toBeVisible();
    await reportSubscriberSidePage.clickOnDataSourcesTab();
    expect(await reportSubscriberSidePage.preferencesTab).toBeVisible();
    await reportSubscriberSidePage.clickOnPreferencesTab();
    expect(await reportSubscriberSidePage.subscribersTab).toBeVisible();
    await reportSubscriberSidePage.clickOnSubscribersTab();
    expect(await reportSubscriberSidePage.scheduleTab).toBeVisible();
    await reportSubscriberSidePage.clickOnScheduleTab();
    await page.waitForTimeout(1000);
    await reportSubscriberSidePage.clickOnCancelButton();
  });
});
