const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class SubscriberMMIProductPage extends BasePage {
  constructor(page) {
    super(page);

    this.confirSubscriberAction = page.locator(`//button[text()='Yes, request to subscribe']`);
    this.confirUnSubscriberAction = page.locator(`//button[text()='Yes, request to unsubscribe']`);
  }

  /**
   * Method to get action button locator for given product
   * @param {string} productName
   * @returns {import('@playwright/test').Locator}
   */
  getProductActionButton(productName) {
    return this.page.locator(`//span[text()='${productName}']//following-sibling::div/label`);
  }

   /**
   * Method to Click on subscrib button
   * @param {string} product name 
   */
  async clickSubscribeButtonFOrGivenProduct(productName) {
    const currentAction = await this.getCancelSubscribedText(productName);
    if (currentAction !== 'SUBSCRIBE') {
      throw new Error(
        `Cannot request subscribe for "${productName}". Expected action "SUBSCRIBE" but found "${currentAction}".`,
      );
    }
    await this.click(this.getProductActionButton(productName));
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
    const currentAction = await this.getCancelSubscribedText(productName);
    if (currentAction !== 'UNSUBSCRIBE') {
      throw new Error(
        `Cannot request unsubscribe for "${productName}". Expected action "UNSUBSCRIBE" but found "${currentAction}".`,
      );
    }
    await this.click(this.getProductActionButton(productName));
    logger.info('Clicked on subscribe button for : ', productName);
    await this.clickYesOnConfirmUnSubscriberPopUp();
  }

  async clickYesOnConfirmUnSubscriberPopUp() {
    await this.click(this.confirUnSubscriberAction);
    logger.info(`Clicked on confirm unsubscriber button`);
  }

  async getCancelSubscribedText(productName) {
    const actionText = (await this.getElementText(this.getProductActionButton(productName))).trim();
    logger.info(`Text for subscribed product is : ${actionText}`);
    return actionText;
  }

}

module.exports = { SubscriberMMIProductPage };
