const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');


class SubscriberNavBarPage extends BasePage {
  constructor(page) {
    super(page);

  }

  /**
   * Method to click on given My Content Section from nav bar
   * @param {string} My Content section name
   */
  async clickOnMyContentSectionByText(navBarOptionName) {
    await this.click(this.page.locator(`//*[@class='group-section']//div[text()='${navBarOptionName}']`));
    logger.info(`Clicked on My Content section option: ${navBarOptionName}`);
    this.hardWait(1000);
  }

}

module.exports = { SubscriberNavBarPage };
