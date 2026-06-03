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
    this.homeMenu = page.locator(`.navbar-brand[href="/contents"]`);
    this.papersSubNavLink = page.locator(`.dropdown-menu a[href="/papers"]`);
    this.transcriptQueueSubNavLink = page.locator(`.dropdown-menu a[href="/transcriptions"]`);

    this.subscriberOther = page.locator('//button[text()="Other"]');
    this.usernameInputOther = page.locator('input#username');
    this.loginButtonOther = page.locator('input[id="kc-login"]');

  }

  /**
   * Login to Editor portal.
   * @param { String } username
   * @param { String } password
   */
  async login(username, password) {
    logger.info(`Clicking on IDIR button to login..`);

    for (let attempt = 0; attempt < 20; attempt++) {
      if (
        (await this.signOutButton.isVisible().catch(() => false)) ||
        (await this.homePageLogo.isVisible().catch(() => false))
      ) {
        logger.info(`Editor session already exists. Continuing without signing in again.`);
        return;
      }

      if (await this.idir.isVisible().catch(() => false)) {
        break;
      }

      await this.page.waitForTimeout(500);
    }

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
    const navigateAndSettle = async (targetUrl) => {
      await this.page.goto(targetUrl, {
        waitUntil: 'domcontentloaded',
        timeout: CONSTANTS.TIMEOUTS.LONG,
      });
      await this.page.waitForLoadState('networkidle', { timeout: CONSTANTS.TIMEOUTS.MEDIUM }).catch(() => {
        logger.info(`Editor URL did not become network idle; continuing after DOM content loaded.`);
      });
    };

    const waitForEditorCallbackToFinish = async () => {
      await this.page
        .waitForURL((currentUrl) => !currentUrl.toString().includes('loginproxy.gov.bc.ca'), {
          timeout: CONSTANTS.TIMEOUTS.LONG,
        })
        .catch(() => {
          logger.info(`Editor URL stayed on loginproxy longer than expected; continuing recovery.`);
        });

      const callbackHashCleared = await this.page
        .waitForFunction(() => !window.location.hash.includes('session_state'), null, {
          timeout: CONSTANTS.TIMEOUTS.LONG,
        })
        .then(() => true)
        .catch(() => false);

      return callbackHashCleared;
    };

    await navigateAndSettle(url);
    await this.hardWait(2000);

    if (!(await this.homePageLogo.isVisible().catch(() => false))) {
      await this.login(process.env.APP_USERNAME, process.env.APP_PASSWORD);
    }

    let callbackHashCleared = await waitForEditorCallbackToFinish();

    if (
      !callbackHashCleared ||
      this.page.url().includes('session_state') ||
      this.page.url().includes('loginproxy.gov.bc.ca')
    ) {
      logger.info(`Reloading clean editor URL after OIDC callback timeout or loginproxy redirect.`);
      await navigateAndSettle(url);

      if (!(await this.homePageLogo.isVisible().catch(() => false))) {
        await this.login(process.env.APP_USERNAME, process.env.APP_PASSWORD);
      }

      callbackHashCleared = await waitForEditorCallbackToFinish();
      if (!callbackHashCleared) {
        logger.info(`Editor callback hash was not fully cleared after clean reload.`);
      }
    }

    if (!(await this.homePageLogo.isVisible().catch(() => false))) {
      await this.homePageLogo.waitFor({ state: 'visible', timeout: CONSTANTS.TIMEOUTS.LONG });
    }
  }

  /**
   * Navigate to Subscriber URL.
   */
  async navigateToSubscriberURL(options = {}) {
    const url = CONSTANTS.URL.SUBSCRIBER_URL;
    const { clearCookies = false } = options;

    if (clearCookies) {
      await this.page.context().clearCookies();
      logger.info(`Cleared cookies to force fresh login`);
    }

    logger.info(`Navigating to URL ${url}`);

    await this.page.goto(url, {
      waitUntil: 'domcontentloaded',
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });

    await this.page.waitForLoadState('networkidle', { timeout: CONSTANTS.TIMEOUTS.MEDIUM }).catch(() => {
      logger.info(`Subscriber URL did not become network idle; continuing after DOM content loaded.`);
    });
    logger.info(`Successfully navigated to URL ${url}`);
  }

  /**
   * Ensure subscriber portal is on a signed-out page before logging in with a requested user.
   */
  async ensureSubscriberLoggedOut() {
    if (await this.subscriberSignOutButton.isVisible().catch(() => false)) {
      logger.info(`Subscriber session already exists. Logging out before signing in.`);
      await this.logOutFromSubscriber();
    }
  }

  /**
   * Login to Subscriber portal.
   * @param { String } user
   * @param { String } password
   */
  async loginAsSubscriber(user, password) {
    logger.info(`Sign in as Subscriber user...`);

    await this.ensureSubscriberLoggedOut();
    await this.click(this.subscriberIdir);
    await this.hardWait(2000);

    await this.type(this.usernameInput, user);
    await this.type(this.passwordInput, password);
    await this.click(this.loginButton);

    await this.page.waitForLoadState('domcontentloaded');
    logger.info(`Clicked on Subscriber Login!!`);
  }
   /**
   * Login to Subscriber portal.
   * @param { String } user
   * @param { String } password
   */
  async loginAsOtherSubscriber(user, password) {
    logger.info(`Sign in as Other Subscriber user...`);

    await this.ensureSubscriberLoggedOut();
    await this.click(this.subscriberOther);
    await this.hardWait(2000);

    await this.type(this.usernameInputOther, user);
    await this.type(this.passwordInput, password);
    await this.click(this.loginButtonOther);

    await this.page.waitForLoadState('domcontentloaded');
    logger.info(`Clicked on Subscriber Login!!`);
  }

  /**
   * Log out from Editor portal
   */
  async logOut() {
    logger.info(`Clicking on SignOut button..`);

    await this.signOutButton.waitFor({ state: 'visible' });
    await this.signOutButton.click({ force: true });
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
      case CONSTANTS.NAVIGATIONMENU.HOME:
        await this.clickElementByText(this.homeMenu, menuName);
        break;
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
      case CONSTANTS.REPORTBUILDING_SUBMENU.FOLDERS:
        await this.clickElementByText(this.subMenuNavigationLink, menuName);
        break;
      case CONSTANTS.REPORTBUILDING_SUBMENU.FILTERS:
        await this.clickElementByText(this.subMenuNavigationLink, menuName);
        break;
      case CONSTANTS.REPORTBUILDING_SUBMENU.EDIT_TOPICS:
        await this.clickElementByText(this.subMenuNavigationLink, menuName);
        break;
      case CONSTANTS.REPORTBUILDING_SUBMENU.DASHBOARD:
        await this.clickElementByText(this.subMenuNavigationLink, menuName);
        break;
      case CONSTANTS.NAVIGATIONMENU.CONTENT_CONFIGURATION:
        await this.click(this.page.locator(`(//a[contains(@class,'nav-link')])[4]`));
        logger.info(`Clicked on ${menuName}`);
        break;
      case CONSTANTS.REPORTBUILDING_SUBMENU.MEDIA_TYPE:
        await this.clickElementByText(this.subMenuNavigationLink, menuName);
        logger.info(`Clicked on ${menuName}`);
        break;
      default:
        logger.info(`Invalid menu or sub menu option: ${menuName}`);
        break;
    }

    await this.hardWait(1500);
  }
}

module.exports = { AppPage };
