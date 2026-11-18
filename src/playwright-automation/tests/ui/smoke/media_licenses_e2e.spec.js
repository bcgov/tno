const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];
const CONSTANTS = require('../../../utils/constants');

let page, appPage, dataImport;

test.beforeEach(async ({ masterFixture }) => {
  appPage = masterFixture.appPage;
  page = masterFixture.page;
  dataImport = masterFixture.dataImport;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(5000);
});

test.describe.serial('@smoke Add and Delete Media License', () => {
  test('Verify adding new media license', async ({ page }) => {
    await page.goto(editorUrl);
    await dataImport.navigateToDataImport();
    await dataImport.navigatetoMediaL();
    await dataImport.clickAddNewLicense();

    const LicenseName = `Automation Test Data`;
    const LicenseDescription = `Automation Description Data for testing `;
    const LicenserandomNmb = Math.floor(Math.random() * 100) + 1;
    const IngestSortOrder = `${LicenserandomNmb}`;
    const licensettl = `${LicenserandomNmb}`;

    await dataImport.enterMediaLicensetDetails(
      LicenseName,
      LicenseDescription,
      IngestSortOrder,
      licensettl,
    );
    await expect(await dataImport.validateMessage()).toBe(true);

    await appPage.logOut();
  });

  test('Search and Delete Media License', async ({ page }) => {
    await page.goto(editorUrl);
    await dataImport.navigateToDataImport();
    await dataImport.navigatetoMediaL();

    const LicenseName = `Automation Test data`;

    await dataImport.searchboxValue(LicenseName);
    await dataImport.clickRow();
    await dataImport.clickDelete();
    await dataImport.removeBtn();
    await dataImport.navigatetoMediaLicense();

    await dataImport.validateDeleteMessage();
    await appPage.logOut();
  });
});
