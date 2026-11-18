const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { expect } = require('@playwright/test');

class PreviewPage extends BasePage {
  constructor(page) {
    super(page);

    this.previewReportTitle = page.locator('#top h1');
    
  }

  /**
   * Method to get the Preview Report title.
   */
  async verifyPreviewReportTitle() {
    logger.info(`Page title is ${await this.getElementText(this.previewReportTitle)}`);
    return await this.getElementText(this.previewReportTitle);
  }

   /**
   * Method to get the Published Headline title visibility.
   */
  async verifyPublishedHeadlineTitleOnPreviewPage(headlinePublished) {
    const headlineTitle = this.page.locator(`//a[text()="${headlinePublished}"]`)
    logger.info(`Is Headline title visible ${await this.isElementVisible(headlineTitle)}`);
    return await this.isElementVisible(headlineTitle);
  }

   /**
   * Method to close the tab.
   */
  async closePage() {
    await this.page.close();
     logger.info(`Closed the new tab`);
  }

  
}

module.exports = { PreviewPage };
