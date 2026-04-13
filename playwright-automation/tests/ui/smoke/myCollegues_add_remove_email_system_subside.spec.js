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

test.describe('@smoke My Collegues add remove email_system_subside', () => {
  test(`Add remove email system subside`, async ({}) => {
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await settingsPage.clickOnSettingsLink();
    await settingsPage.clickOnMyColleaguesLink();
    await settingsPage.enterColleagueEmail('priya.saini@gov.bc.ca');
    await settingsPage.clickOnAddColleagueButton();
    await settingsPage.hardWait(2000);
    expect(page.getByText("Successfully added 'priya.saini@gov.bc.ca'")).toBeVisible();
    await settingsPage.clickOnRemoveColleagueButton();
    await settingsPage.hardWait(2000);
    expect(page.getByText("Successfully deleted 'priya.saini@gov.bc.ca'")).toBeVisible();
  });
});
