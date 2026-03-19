const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');
const CONSTANTS = require('../utils/constants');

class AppPage extends BasePage {
  constructor(page) {
    super(page);
    this.idir = page.locator('//button[./div[normalize-space()="IDIR"]]');
    this.usernameInput = page.locator('input#user');
    this.passwordInput = page.locator('input#password');
    this.loginButton = page.locator('input[name="btnSubmit"]');
    this.homePageLogo = page.locator('//a/img[@class="app-logo"]');
    this.signOutButton = page.locator('//button[@name="signOut"]');
    this.subscriberSignOutButton = page.locator('div.logout');

    this.subscriberIdir = page.locator('button.idir-logo');
    this.menuNavigationLink = page.locator('div.nav-item .dropdown-toggle');
    this.subMenuNavigationLink = page.locator('.show a.dropdown-item');
    this.allContentSubNavLink = page.locator(`.dropdown-menu a[href="/contents"]`);
<<<<<<< HEAD

    this.microsoftButton = page.getByRole('button', { name: 'Microsoft' });

this.loginEmailInput = page.getByRole('textbox', { name: 'Enter your email or phone' });

this.nextButton = page.getByRole('button', { name: 'Next' });

this.passwordInputField = page.locator('input[type="password"]');

this.signInButton = page.getByRole('button', { name: 'Sign in' });

this.noButton = page.getByRole('button', { name: 'No' });
=======
    this.papersSubNavLink = page.locator('.dropdown-menu a[href="/papers"]');
     this.transcriptQueueSubNavLink = page.locator('.dropdown-menu a[href="/transcriptions"]');
>>>>>>> dev
  }

  /**
   * Login to Editor portal.
   * @param { String } username
   * @param { String } password
   */
  async login(username, password) {
    logger.info(`Clicking on IDIR button to login..`);

    await this.click(this.idir);
    await this.type(this.usernameInput, username);
    await this.type(this.passwordInput, password);
    await this.click(this.loginButton);

    logger.info(`Clicked on Login!!`);
  }






  

  






  /**
   * Method to navigate to given URL
   * @param {string} url 
   */
  async navigateToUrl(url) {
    logger.info(`Navigating to URL : ${url}`);
    await this.page.goto(url);
    await this.hardWait(2000);
    if (!(await this.homePageLogo.isVisible())) {
      try {
        this.login(process.env.app_username, process.env.app_password);
        await this.homePageLogo.waitFor({ state: 'visible' });
      } catch (error) {
        console.log(error);
      }
    }
  }

  /**
   * Navigate to Subscriber URL.
   */
  async navigateToSubscriberURL() {
    const url = CONSTANTS.URL.SUBSCRIBER_URL;

    logger.info(`Navigating to URL ${url}`);

    await this.page.goto(url, {
      waitUntil: 'domcontentloaded',
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });

    await this.page.waitForLoadState('networkidle');
    logger.info(`Successfully navigated to URL ${url}`);
  }

  /**
   * Login to Subscriber portal.
   * @param { String } user
   * @param { String } password
   */
  async loginAsSubscriber(user, password) {
    logger.info(`Sign in as Subscriber user...`);

    await this.click(this.subscriberIdir);
    await this.hardWait(2000);

    await this.type(this.usernameInput, user);
    await this.type(this.passwordInput, password);
    await this.click(this.loginButton);

    await this.page.waitForLoadState('domcontentloaded');
    logger.info(`Clicked on Subscriber Login!!`);
  }

  /**
   * Log out from Editor portal
   */
  async logOut() {
    logger.info(`Clicking on SignOut button..`);

    await this.click(this.signOutButton);
    await this.isElementVisible(this.idir);
    await this.page.waitForLoadState('networkidle');

    logger.info(`Successfully logged out!!`);
  }

  /**
   * Log out from Subscriber portal
   */
  async logOutFromSubscriber() {
    logger.info(`Clicking on SignOut button..`);

    await this.click(this.subscriberSignOutButton);
    await this.page.waitForLoadState('networkidle');

    logger.info(`Successfully logged out from Subscriber portal!!`);
  }



  /**
   * Login to MMI Editor Portal using Microsoft SSO.
   * @param { String } username
   * @param { String } password
   */
  async mmiMicrooftLogin(username, password) {
    logger.info(`Clicking on Microsoft button to login..`);
    await this.microsoftButton.click();
    await this.loginEmailInput.type(username);
    await this.nextButton.click();
    await this.page.waitForTimeout(2000);
    await this.passwordInputField.type(password);
    await this.signInButton.click();
    await this.noButton.click();
    logger.info(`Login MMI Editor Portal using Microsoft SSO!!`);

  }




  /**
   * Method to navigate to given URL
   * @param {string} url 
   */
  async navigateToMMIUrl(url) {
    logger.info(`Navigating to URL : ${url}`);
    await this.page.goto(url);
    await this.hardWait(2000);
    if (!(await this.homePageLogo.isVisible())) {
      try {
        this.mmiMicrooftLogin(process.env.app_username_MMI, process.env.app_password_MMI);
        await this.homePageLogo.waitFor({ state: 'visible' });
      } catch (error) {
        console.log(error);
      }
    }
  }





  /**
   * Method to click on given Navigation and Sub Navigation menu
   * @param {string} menuName
   */
  async clickOnMenuAndSubNavigationMenuLink(menuName) {
    logger.info(`Clicking on Menu navigation dropdown option ${menuName}`);
    switch (menuName) {
      case CONSTANTS.NAVIGATIONMENU.REPORT_BUILDING:
        await this.clickElementByText(this.menuNavigationLink, menuName);
        break;
      case CONSTANTS.REPORTBUILDING_SUBMENU.REPORTS:
        await this.clickElementByText(this.subMenuNavigationLink, menuName);
        break;
      case CONSTANTS.REPORTBUILDING_SUBMENU.MMI_PRODUCTS:
        await this.clickElementByText(this.subMenuNavigationLink, menuName);
        break;
      case CONSTANTS.NAVIGATIONMENU.CONTENT:
        await this.clickElementByText(this.menuNavigationLink, menuName);
        break;
      case CONSTANTS.CONTENT_SUBMENU.ALL_CONTENT:
        await this.clickElementByText(this.allContentSubNavLink, menuName);
        break;
      case CONSTANTS.CONTENT_SUBMENU.PAPERS:
        await this.clickElementByText(this.papersSubNavLink, menuName);
        break;
      case CONSTANTS.CONTENT_SUBMENU.TRANSCRIPT_QUEUE:
        await this.clickElementByText(this.transcriptQueueSubNavLink, menuName);
        break;
      default:
        logger.info(`Invalid menu or sub menu option: ${menuName}`);
        break;
    }
  }
}

module.exports = { AppPage };
