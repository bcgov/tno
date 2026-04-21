const { describe } = require('node:test');
const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, notificationpage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  notificationpage = masterFixture.notificationpage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Verify Passed Notifications in Notification Dashboard', () => {
  test(`Verify Passed Notifications `, async ({}) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
    await page.goto(`${mmiMSUrl}admin/notifications/dashboard`);
    await notificationpage.selectNotification("Top Stories")
    const today = new Date();
const endDate = today.toLocaleDateString("en-US");

const lastYear = new Date();
lastYear.setFullYear(today.getFullYear() - 1);
const startDate = lastYear.toLocaleDateString("en-US");
    await notificationpage.enterNotificationDates(startDate,endDate)
    await notificationpage.uncheckNotificationCheckbox()
    await notificationpage.clickNotificationSearchButton()
   const count = await page.locator('[class="notification-card"]').count();
console.log(`Rows count: ${count}`);
    
  });
});
