const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class SubscriberMMIProductPage extends BasePage {
  constructor(page) {
    super(page);

    this.confirSubscriberAction = page.locator(`//button[text()='Yes, request to subscribe']`);
    this.confirUnSubscriberAction = page.locator(`//button[text()='Yes, request to unsubscribe']`);
  }


   /**
   * Method to Click on subscrib button
   * @param {string} product name 
   */
  async clickSubscribeButtonFOrGivenProduct(productName) {
    await this.click(this.page.locator(`//span[text()='${productName}']//following-sibling::div/label`));
    logger.info(`Clicked on subscribe button for : ${productName}`);
    await this.clickYesOnConfirmSubscriberPopUp();
    await this.hardWait(3000);
  }

  async clickYesOnConfirmSubscriberPopUp() {
    await this.click(this.confirSubscriberAction);
    logger.info(`Clicked on confirm subscriber button`);
  }

  /**
   * Method to Click on subscrib button
   * @param {string} product name 
   */
  async clickUnSubscribeButtonForGivenProduct(productName) {
    await this.click(this.page.locator(`//span[text()='${productName}']//following-sibling::div/label`));
    logger.info('Clicked on subscribe button for : ', productName);
    await this.clickYesOnConfirmUnSubscriberPopUp();
  }

  async clickYesOnConfirmUnSubscriberPopUp() {
    await this.click(this.confirUnSubscriberAction);
    logger.info(`Clicked on confirm unsubscriber button`);
  }

  async getCancelSubscribedText(productName) {
    logger.info(`Text for subscribed product is : ${await this.getElementText(this.page.locator(`//span[text()='${productName}']//following-sibling::div/label`))}`);
    return await this.getElementText(this.page.locator(`//span[text()='${productName}']//following-sibling::div/label`));
  }

}

module.exports = { SubscriberMMIProductPage };
