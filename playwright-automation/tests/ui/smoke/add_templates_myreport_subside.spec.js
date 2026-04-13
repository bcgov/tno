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

test.describe('@smoke Add Templates to My Report Subscriber Side', () => {
  test(`Add Templates to My Report and cancel`, async ({}) => {
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
    await reportSubscriberSidePage.clickOnTemplateTab();
    expect(await reportSubscriberSidePage.tableOfContents).toBeVisible();
    expect(await reportSubscriberSidePage.aiTab).toBeVisible();
    await reportSubscriberSidePage.clickOnTableOfContents();
    await reportSubscriberSidePage.clickOnAiTab();
    expect(await reportSubscriberSidePage.templateStories).toBeVisible();
    await reportSubscriberSidePage.clickOnTemplateStories();
    expect(await reportSubscriberSidePage.templateMediaAnalytics).toBeVisible();
    await reportSubscriberSidePage.clickOnTemplateMediaAnalytics();
    expect(await reportSubscriberSidePage.templateText).toBeVisible();
    await reportSubscriberSidePage.clickOnTemplateText();
    expect(await reportSubscriberSidePage.templateFrontPageImages).toBeVisible();
    await reportSubscriberSidePage.clickOnTemplateFrontPageImages();
    expect(await reportSubscriberSidePage.templateImage).toBeVisible();
    await reportSubscriberSidePage.clickOnTemplateImage();
    expect(await reportSubscriberSidePage.templateData).toBeVisible();
    await reportSubscriberSidePage.clickOnTemplateData();
    await page.waitForTimeout(2000);
    await reportSubscriberSidePage.clickOnCancelButton();
  });
});
