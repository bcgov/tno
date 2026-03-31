const { describe } = require('node:test');
const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];

let page, appPage, showProgramPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  showProgramPage = masterFixture.showProgramPage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Add and Remove Show Program', () => {
  test(`Add and Remove Show Program `, async ({  }) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
        await page.goto(`${mmiMSUrl}admin/programs`);
        await showProgramPage.clickOnAddShowProgramButton();
        await showProgramPage.enterName('Test Show Program');
        await showProgramPage.selectSource('TNO 1.0 (TNO)')
        await showProgramPage.enterDescription('This is a test show program created by automation script.');
        await showProgramPage.selectMediaType('News Radio');
        await page.waitForTimeout(2000);
        await showProgramPage.enterProgramSortOrder(1);
        await showProgramPage.toggleEnabledCheckbox();
        expect(await showProgramPage.enabledCheckbox.isChecked()).toBeTruthy();
        await showProgramPage.toggleAutomaticallyTranscribeCheckbox();
        expect(await showProgramPage.automaticallyTranscribeCheckbox.isChecked()).toBeTruthy();
        await showProgramPage.clickOnSaveButton();  
        expect(page.getByText('Test Show Program has successfully been saved.')).toBeVisible();
        await page.waitForTimeout(10000);
        await showProgramPage.clickOnBackToShowProgramButton();
        await page.waitForTimeout(2000);
        await showProgramPage.searchShowProgramByName('Test Show Program');
        await showProgramPage.clickProgramRecord('Test Show Program');
        await showProgramPage.deleteShowProgram();
        await showProgramPage.searchShowProgramByName('Test Show Program');
        await expect(page.locator('.rows')).not.toContainText('Test Show Program');
  })
})
