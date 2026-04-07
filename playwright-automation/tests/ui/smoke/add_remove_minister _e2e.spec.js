const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const mmiMSUrl = testData[testApp]['microsoftMMI']['url'];
        let ministertName = 'Adrian Dix :';

let page, appPage, editorOnlineStoryPage, subscriberNavBarPage, ministerPage;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  editorOnlineStoryPage = masterFixture.editorOnlineStoryPage;
  ministerPage = masterFixture.ministerPage;
  await appPage.navigateToMMIUrl(mmiMSUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Add and Remove Minister ', () => {

  test(`Adding a minister`, async ({}) => {

    await expect(page).toHaveURL(mmiMSUrl + 'contents');
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await ministerPage.clickOnSettings();
    await ministerPage.clickOnMyMinister();
    await ministerPage.clickOnMinisterCheckbox(ministertName);
    await ministerPage.clickOnMinisterSaveButton();
    await ministerPage.clickMyMinisterLink()
    const expectedText = ministertName.replace(':', '(0)');
    await expect(page.getByText(expectedText)).toBeVisible();

  });

  test(`removing a minister`, async ({}) => {
    await expect(page).toHaveURL(mmiMSUrl + 'contents');
    await page.goto(`${process.env.MMI_URL}/landing/home`);
    await ministerPage.clickOnSettings();
    await ministerPage.clickOnMyMinister();
    await ministerPage.unClickOnMinisterCheckbox(ministertName);
    await ministerPage.clickOnMinisterSaveButton();
    await ministerPage.clickMyMinisterLink()
    const expectedText = ministertName.replace(':', '(0)');
    await expect(page.getByText(expectedText)).not.toBeVisible();

  });
});
