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
      default:
        logger.info(`Invalid menu or sub menu option: ${menuName}`);
        break;
    }
  }
}

module.exports = { AppPage };
