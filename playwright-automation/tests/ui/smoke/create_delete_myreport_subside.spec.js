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

test.describe('@smoke Create and Delete My Report Subscriber Side', () => {
  test(`Create and Delete My Report`, async ({}) => {
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await reportSubscriberSidePage.clickOnMyreportsLink();
    expect(await reportSubscriberSidePage.newReportButton).toBeVisible();
    await reportSubscriberSidePage.clickOnNewReportButton();
    await page.waitForTimeout(2000);
    const randomNum = Math.floor(Math.random() * 1000);

    await reportSubscriberSidePage.enterReportName('Test Report' + randomNum);
    await reportSubscriberSidePage.enterReportDescription(
      'This is a test report created by automation script.',
    );
    await reportSubscriberSidePage.enterReportSubjectLineEmail('priya@example.com');
    await reportSubscriberSidePage.clickOnReportSaveButton();
    await page.waitForTimeout(2000);
    expect(page.getByText(`Successfully created 'Test Report${randomNum}'`)).toBeVisible();
    await page.waitForTimeout(2000);

    await reportSubscriberSidePage.clickOnVisibledata();
    const sections = page.locator('.section-label');
    const count = await sections.count();

    for (let i = 0; i < count; i++) {
      const text = await sections.nth(i).textContent();

      if (text && text.includes(`ManualTest Report${randomNum}`)) {
        await sections.nth(i).locator(`text=Report${randomNum}`).click();
        await reportSubscriberSidePage.clickOnDelete();
        break;
      }
    }
  });
});
