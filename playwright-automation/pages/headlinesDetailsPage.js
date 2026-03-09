const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { expect } = require('@playwright/test');

class HeadlinesDetailsPage extends BasePage {
  constructor(page) {
    super(page);

    this.headlinesTextBox = page.locator('textarea#txa-headline');
    this.byLineInput = page.locator('input#txt-byline');
    this.publishButton = page.locator('//button[@type="submit"]');
    this.toastNotification = page.locator('.Toastify .Toastify__toast-body div:nth-child(2)');
  }

  /**
   * Method to check Headlines details page is loaded.
   */
  async verifyHeadlinesDetailsPageLoaded() {
    await this.hardWait(5000);
    await expect(this.byLineInput).toBeVisible({timeout: CONSTANTS.TIMEOUTS.LONG });
    logger.info('Headlines details page loaded successfully!!');
  }

  /**
   * Methos to get the text from Headlines text field
   * @returns Headlines title
   */
  async getHeadlinesTextFieldValue() {
    await this.hardWait(2000);
    const headlinesTitle = await this.getInputValue(this.headlinesTextBox);
    logger.info(`Headlines title is ${headlinesTitle}`);
    return headlinesTitle;
  }

  /**
   * Method to enter value in byline text field
   * @param {String} bylineText
   */
  async enterByline(bylineText) {
    await this.type(this.byLineInput, bylineText);
    logger.info(`Entered byline text ${bylineText}`);
  }

  /**
   * Method to click on Publish button.
   */
  async publishHeadlines() {
    await this.click(this.publishButton);
    logger.info(`Clicked on Publish button`);
  }

  /**
   * Method to click on Sentiment button
   * @param {String} value 
   */
  async clickOnSentimentButtonByText(value) {
    await this.click(this.page.locator(`//*[@label="Sentiment"]//button[text()="${value}"]`));
    logger.info(`Clicked on Sentiment button ${value}`);
  }

  /**
   * Method to check Totast Notification's visibility
   * @param {String} headlinesTitle 
   * @returns true is Toast Notification is visible else false
   */
  async verifyToastNotificationVisible(headlinesTitle) {
    logger.info(`Verifying visibility of Toast Notification Message`);
    await expect(this.toastNotification.first()).toHaveText(`"${headlinesTitle}" has successfully been saved.`)
    return await this.isElementVisible(this.toastNotification.first());
  }

  /**
   * Method to close the tab.
   */
  async closePage() {
    await this.page.close();
     logger.info(`Closed the new tab`);
  }
}

module.exports = { HeadlinesDetailsPage };
