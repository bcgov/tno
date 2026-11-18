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

test.describe.serial('@smoke Add and Delete Ingest Type', () => {
  test('Verify adding new ingest type', async ({ page }) => {
    await page.goto(editorUrl);
    await dataImport.navigateToDataImport();
    await dataImport.navigateToIngest();
    await dataImport.clickAddNewIngest();

    const IngestName = `Automation Test Data`;
    const IngestDescription = `Automation Description Data for testing `;
    const randomNmb = Math.floor(Math.random() * 100) + 1;

    await dataImport.enterIngestDetails(IngestName, IngestDescription, randomNmb);
    await dataImport.dropdownOption('PrintContent');

    await expect(await dataImport.validateMessage()).toBe(true);
    await dataImport.backbtnIngest();

    await appPage.logOut();
  });

  test('Search and Delete Ingest Type', async ({ page }) => {
    await page.goto(editorUrl);
    await dataImport.navigateToDataImport();
    await dataImport.navigateToIngest();
    await page.waitForTimeout(5000);

    const ingestName = `Automation Test data`;
    await dataImport.searchboxValue(ingestName);
    await dataImport.clickonrowValue();
    await dataImport.clickDelete();
    await dataImport.removeBtn();
    await dataImport.navigatetotypes();

    await dataImport.validateDeleteMessage();
    await appPage.logOut();
  });
});
