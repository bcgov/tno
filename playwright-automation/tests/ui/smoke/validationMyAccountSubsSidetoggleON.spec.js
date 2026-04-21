const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, settingsPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  settingsPage = masterFixture.settingsPage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);

});

test.describe('@smoke Validation MyAccount Subscriber side toggle notifaction on', () => {
  test(`Validation MyAccount Subscriber side toggle notifaction on`, async ({}) => {
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await settingsPage.clickOnSettingsLink();
    await settingsPage.clickOnMyAccountLink();
    await settingsPage.toggleEmailNotificationOn();
    await settingsPage.toggleReportSentimentOn();
    expect(await settingsPage.emailToggleButton.getAttribute('value')).toBe('true');
    expect(await settingsPage.reportsentimentToggleButton.getAttribute('value')).toBe('true');

  })
})