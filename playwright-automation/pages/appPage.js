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
    this.signOutButton = page.locator('//button[@name="signOut"]');

    this.subscriberIdir = page.locator('button.idir-logo');
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
    
    logger.info(`Enter username as ${user}`);
    await this.type(this.usernameInput, user);
     logger.info(`Enter password as ${password}`);
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
}

module.exports = AppPage;
