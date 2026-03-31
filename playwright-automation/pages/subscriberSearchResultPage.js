const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');

class SubscriberSearchResultPage extends BasePage {
  constructor(page) {
    super(page);

    this.headlines = page.locator('.grouped-content .headline');
    this.searchButtonOnTop = page.locator('(//div[text()="Search"]/parent::button)[2]');
    this.pageErrorMessage = page.locator(`//h1[text()="Something went wrong."]`);
  }

  /**
   * Method to check Headlines details page is loaded.
   */
  async verifySearchResultPageLoaded() {
    logger.info('Waiting for Headlines details page to be loaded..');
    await this.headlines.first().waitFor({ state: 'visible', timeout: CONSTANTS.TIMEOUTS.LONG });
    await this.headlines.first().waitFor({ state: 'attached', timeout: CONSTANTS.TIMEOUTS.LONG });
    logger.info('Headlines details page loaded successfully!!');
  }

  async clickOnSearchButton() {
    await this.click(this.searchButtonOnTop);
    logger.info('Clicked on seach button on the top');
    await this.verifySearchResultPageLoaded();
  }

  /**
   * Method to check if headline is present in the searched result
   * @param {string} headlineTitle 
   * @returns {boolean} true if present else false
   */
  async isPublishedHeadlinesPresent(headlineTitle) {
    logger.info('Verifying published headlines in search results..');
    return this.isTextPresentInCollection(this.headlines, headlineTitle);
  }

  /**
   * Method to check if error message is present in the searched result
   * @returns {boolean} true if present else false
   */
  async isPageErrorMessagePresent() {
    logger.info('Verifying Error Message..');
    return this.isElementVisible(this.pageErrorMessage);
  }

}

module.exports = { SubscriberSearchResultPage };
