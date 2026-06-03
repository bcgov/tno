const { chromium } = require('@playwright/test');

const { loadEnv, requireEnv } = require('./env');

//global set up function to set up base url as per target environment passed on CLI
async function globalSetup() {
  loadEnv();
  requireEnv(['EDITOR_URL', 'APP_USERNAME', 'APP_PASSWORD']);

  const env_name = process.env.ENV_NAME;
  const app_name = process.env.APP_NAME;
  const login_url = process.env.LOGIN_URL;

  console.log('process.env.ENV_NAME: ' + process.env.ENV_NAME);
  console.log('process.env.APP_NAME: ' + process.env.APP_NAME);
  console.log('process.env.LOGIN_URL: ' + process.env.LOGIN_URL);

  const browser = await chromium.launch();
  const context = await browser.newContext({viewport: null});
  const page = await context.newPage();

  //  Capture Network traffic
  // await (await page).on('request', request => console.log('>>>>', request.method(), request.url()));
  // await (await page).on('response', response => console.log('<<<<', response.status(), response.url()));
  // Set up event listeners to capture network traffic
  // await (await page).on('request', request => {
  //   console.log(`>> ${request.method()} ${request.url()}`);
  //   console.log(`Headers: ${JSON.stringify(request.headers())}`);
  // });

  // await (await page).on('response', async response => {
  //   console.log(`<< ${response.status()} ${response.url()}`);
  //   console.log(`Headers: ${JSON.stringify(response.headers())}`);
  // });

 await page.goto(login_url, { timeout: 10000 });
  //await page.waitForLoadState ('documentloaded');
  const idir = page.locator('//button[./div[normalize-space()="IDIR"]]');
  const IdirUserName = page.locator('input#user');
  const IdirPassword = page.locator('input#password');
  //const continueButton = page.locator("//a/img[@class='app-logo']");
  const continueButton = page.locator('input[name="btnSubmit"]');

  // Only perform IDIR login operation if presented with login page else skip login process

   const idirVisible = await idir.isVisible().catch(() => false) ;
   if (idirVisible) {
    try {
      await idir.click();
      await IdirUserName.fill(process.env.APP_USERNAME);
      await IdirPassword.fill(process.env.APP_PASSWORD);
      await continueButton.click();
     //await homePageLogo.waitFor({ state: 'visible' });
    } catch (error) {
      console.log(error);
    }
  }
  await (await page).context().storageState({ path: "/tmp/login.json" });
  await browser.close();

}

module.exports = globalSetup;
