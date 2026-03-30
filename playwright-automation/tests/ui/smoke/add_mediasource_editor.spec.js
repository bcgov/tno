const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, sourcePage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  sourcePage = masterFixture.sourcePage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Add media source', () => {
  test(`Add media source`, async ({  }) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
        await page.goto(`${mmiMSUrl}admin/sources`);

        await sourcePage.clickOnAddSourceButton();
        const randomNum = Math.floor(Math.random() * 10000);
        const legalName = 'Test Media Source'+randomNum;
        await sourcePage.enterLegalName(legalName);
        await sourcePage.enterCode('TMS'+randomNum);
        await sourcePage.selectLicense('One year');
        await sourcePage.toggleEnabledCheckbox();
        await sourcePage.toggleUseInTopicsCheckbox();
        await sourcePage.toggleDisableTranscriptRequestsCheckbox();
        await sourcePage.clickOnSaveButton(); 
        await page.waitForTimeout(10000);
        await sourcePage.clickOnBackToSourcesButton();
        await page.waitForTimeout(2000);
        await sourcePage.searchLeagelName(legalName);
        await page.waitForTimeout(2000);
        await expect(page.getByText('Test Media Source'+randomNum)).toBeVisible();
       
  })
})