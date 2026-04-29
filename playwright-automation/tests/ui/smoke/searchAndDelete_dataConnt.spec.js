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
     console.log("Actions",dataImport );
});
test.describe('@smoke Data Connections', () => {
     test(`Verify Search and Delete for Data Connections`, async ({page}) => {
     
    await page.goto(editorUrl);
    await dataImport.navigateToDataImport();
    await dataImport.navigateToDataCnctn();
    
    const keyword = `Automation Test Data`;
    
    await dataImport.searchboxValue(keyword);
     console.log(" Fetch Value:", keyword);
     await dataImport.clickonrowValue();
      console.log("Click on Row");
    await dataImport.clickDelete();
     console.log(`Deletion of ${keyword} is successful.`);
    await dataImport.removeBtn(); 
    
    await dataImport.navigatetoConnection();
     console.log("Navigate to Data Connections");
     await dataImport.validateDeleteMessage();
      console.log(`Deletion of ${keyword} is successful.`);
    
    await appPage.logOut();
  });
});