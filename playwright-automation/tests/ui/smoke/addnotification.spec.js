const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

console.log("Editor URL:",editorUrl );

let page, appPage, notificationalertPage;

test.beforeEach(async ({ masterFixture }) => {
    page = masterFixture.page;  
    appPage = masterFixture.appPage;
    notificationalertPage = masterFixture.notificationalertPage;
    await appPage.navigateToUrl(editorUrl);
    await appPage.hardWait(5000);
});

test.describe('@smoke Notification Module', () => {
     test(`Notification Builder`, async ({  }) => {

    await expect(page).toHaveURL(editorUrl);  
    await notificationalertPage.navigateNotifBuild(); 
    await notificationalertPage.notificationMenue();
    await notificationalertPage.addnotification();
    const randomNum = Math.floor(Math.random() * 10000);
    const notificationName = `Auto Notification : ${randomNum}`;
    const notificationDescription = `Auto Notification Description :${randomNum}`;
    const ministerSortOrder = 4;

    await notificationalertPage.enterDetails(
      notificationName,
      notificationDescription,
      ministerSortOrder,
    );

    await appPage.logOut();
     });
    });