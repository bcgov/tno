import { chromium } from "@playwright/test";
//const { chromium } = require('@playwright/test');
require('dotenv').config();

//global set up function to set up base url as per target environment passed on CLI
async function globalSetup() {

  let env_name = process.env.TARGET_ENV;  // Get ENV_NAME from Command Line
  let app_name = process.env.TARGET_APP;  // Get APP_NAME from Command Line
  if (env_name === undefined) {
    env_name = 'test';  // Default to Test/UAT envt.
  };
  if (app_name === undefined) {
    app_name = 'qa';  // Default to Rendering Application QA app.
  };
  // Load enviroment variables in env file
  const dotenv = require("dotenv");
  dotenv.config({
    path: `.env.${env_name}`,
    override: true,
  });
  // LOGIN_URL value can be passed via npx command. for e.g LOGIN_URL='https://test.editor.mmi.gov.bc.ca/'. Else value is null/undefined. 
  let login_url = process.env.LOGIN_URL;
  if (login_url === undefined) {
    // Use url in .env file based on application under test
    if (app_name == 'cmsl') {
      login_url = process.env.CMSL_URL;
    } else if (app_name == 'qa') {
      login_url = process.env.QA_URL;
    } else if (app_name == 'rt') {
      login_url = process.env.RT_URL;
    } else {
      login_url = process.env.CMSL_URL;
    }
  };
  // Save env_name, app_name and login_url as env vairables, can be used later
  process.env.ENV_NAME = env_name;
  process.env.APP_NAME = app_name;
  process.env.LOGIN_URL = login_url;

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

  await page.goto(process.env.LOGIN_URL, { timeout: 10000 });
  //await page.waitForLoadState ('documentloaded');
  const idir = page.locator('//button[./div[normalize-space()="IDIR"]]');
  const IdirUserName = page.locator('input#user');
  const IdirPassword = page.locator('input#password');
  //const continueButton = page.locator("//a/img[@class='app-logo']");
  const continueButton = page.locator('input[name="btnSubmit"]');

  // Only perform IDIR login operation if presented with login page else skip login process
 // if (!(await homePageLogo.isVisible())) 
    
   const logoVisible = await page.isVisible().catch(() => false) ;
   if (! logoVisible) {
    try {
      await idir.click();
      await IdirUserName.fill(process.env.app_username);
      await IdirPassword.fill(process.env.app_password);
      await continueButton.click();
      await homePageLogo.waitFor({ state: 'visible' });
    } catch (error) {
      console.log(error);
    }
  }
  await (await page).context().storageState({ path: "/tmp/login.json" });
  await browser.close();

}

module.exports = globalSetup;