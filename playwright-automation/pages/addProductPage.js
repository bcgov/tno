const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');
const { expect } = require('@playwright/test');

class AddProductPage extends BasePage {
  constructor(page) {
    super(page);

    this.backToProductButton = page.getByRole('button', { name: 'Back to Products' });
    this.nameInput = page.getByPlaceholder('Enter unique product name');
    this.productTypeDropdown = page.locator(`input[id*='react-select'][id$='-input']`).nth(0);
    this.targetProductDropdown = page.locator(`input[id*='react-select'][id$='-input']`).nth(1);
    this.saveButton = page.getByRole('button', { name: 'Save' });

    this.deleteButton = page.getByRole('button', { name: 'Delete' });
    this.deleteConfirmation = page.locator('//button/div[text()="Yes, Remove It"]');

    this.searchSubscriberInput = page.getByPlaceholder('Search by keyword');
    this.searchSubscriberButton = page.locator(`//button//*[@alt="search"]`);
    this.subscriberRecordCheckbox = page.locator(`.grid-column  input[type="checkbox"]`);

    this.productLink = page.locator(`.link`);

    this.userName = page.locator(`.user-name`);
    this.approveButton = page.locator(`//button//div[text()='Approve']`);
    this.rejectButton = page.locator(`//button//div[text()='Reject']`);

  }

  /**
   * Method to check visibility of back to products button.
   */
  async verifyBackToProductsVisibility() {
    logger.info(`Visibility of button is ${await this.isElementVisible(this.backToProductButton)}`);
    return await this.isElementVisible(this.backToProductButton);
  }

  /**
   * Method to enter Product detail
   * @param {string} name
   * @param {string} productType
   * @param {string} targetProduct
   */
  async enterProductDetails(name, productType, targetProduct) {
    await this.type(this.nameInput, name);

    await this.selectReactOption(this.productTypeDropdown, productType);
    await this.selectReactOption(this.targetProductDropdown, targetProduct);

    await this.verifyReactValueSelected(productType);
    await this.verifyReactValueSelected(targetProduct);

    logger.info(`Added Product Details.`);
  }

  /**
   * Select an option from a react-select input.
   * @param {import('@playwright/test').Locator} input
   * @param {string} optionText
   */
  async selectReactOption(input, optionText) {
    await this.type(input, optionText);
    const option = this.page.getByRole('option', { name: optionText, exact: true }).first();
    if (await this.isElementVisible(option, 5000)) {
      await this.click(option);
    } else {
      await this.page.keyboard.press('Enter');
    }
    logger.info(`Selected react option : ${optionText}`);
  }

  /**
   * Verify the selected react-select value is visible in a control.
   * @param {string} optionText
   */
  async verifyReactValueSelected(optionText) {
    const selectedValue = this.page
      .locator('.rs__single-value, .rs__multi-value__label')
      .filter({ hasText: optionText })
      .first();
    await expect(selectedValue).toBeVisible();
  }

  /**
   * Method to save the Product details.
   */
  async save(){
    await this.click(this.saveButton);
    await this.hardWait(2000);
    logger.info(`Clicked on Save Button`);
  }

  /**
   * Save product details and verify the save toast before leaving the form.
   * @param {string} productName
   */
  async saveAndVerifyProductSaved(productName) {
    await this.save();
    const matchingToast = this.page
      .locator('.Toastify .Toastify__toast-body div:nth-child(2)')
      .filter({ hasText: productName })
      .first();
    await expect(matchingToast).toContainText(productName);
    await expect(matchingToast).toContainText('has successfully been saved.');
    logger.info(`Verified product save toast for : ${productName}`);
  }

  /**
   * Method to click on given product subtab
   * @param {string} subTab Name
   */
  async clickOnProductSubTab(tabName) {
    await this.click(
      this.page.locator(`//div[contains(@class,'tab-menu')]//span[text()="${tabName}"]`),
    );
    logger.info(`Clicked on SubTab : ${tabName}`);
  }

  /**
   * Method to select subscriber details
   * @param {string} subscriberName 
   */
  async selectSubscriber(subscriberName) {
    await this.type(this.searchSubscriberInput, subscriberName);
    await this.click(this.searchSubscriberButton);
    await this.hardWait(1000);

    await this.click(this.subscriberRecordCheckbox);
    logger.info(`Performed search for ${subscriberName}`)
  }

  /**
   * Method to go back to Product grid
   */
  async backToProductGrid() {
    await this.click(this.backToProductButton);
    logger.info(`Clicked on Back to Product button`);
  }

  /**
   * Method to check visibility of product on the grid
   * @param {string} productName 
   * @returns true if visible
   */
  async isAddedProductVisibleOnGrid(productName) {
    await this.clear(this.searchSubscriberInput);
    await this.type(this.searchSubscriberInput, productName);
    await this.page.keyboard.press('Enter');
    const productLink = this.page.locator('.link', { hasText: productName }).first();
    const isProductVisible = await this.isElementVisible(productLink, 10000);
    logger.info(`Is Added product Visible : ${isProductVisible}`);
    return isProductVisible;
  }

  /**
   * Method to select given produc
   * @param {string} productName 
   */
  async selectProduct(productName) {
    await this.clear(this.searchSubscriberInput);
    await this.type(this.searchSubscriberInput, productName);
    await this.page.keyboard.press('Enter');
    const productLink = this.page.locator('.link', { hasText: productName }).first();
    await productLink.waitFor({ state: 'visible', timeout: 10000 });
    logger.info(`Total product shown on grid : ${await this.productLink.count()}`);
    await this.click(productLink);
    logger.info(`Selected product : ${productName}`);
  }

  /**
   * Method to delete the product
   */
  async deleteProduct() {
    await this.click(this.deleteButton);
    await this.click(this.deleteConfirmation);
    logger.info(`Successfully Deleted the newly added product.`);
  }

  /**
   * Method to check visibility of user name.
   */
  async verifyUserNameVisibility() {
    logger.info(`Visibility of user name  is ${await this.isElementVisible(this.userName)}`);
    return await this.isElementVisible(this.userName);
  }

   /**
   * Method to check visibility of Approve button.
   */
  async verifyApproveButtonVisibility() {
    logger.info(`Visibility of Approve button is ${await this.isElementVisible(this.approveButton)}`);
    return await this.isElementVisible(this.approveButton);
  }

  /**
   * Method to check visibility of Reject button.
   */
  async verifyRejectButtonVisibility() {
    logger.info(`Visibility of Reject button is ${await this.isElementVisible(this.rejectButton)}`);
    return await this.isElementVisible(this.rejectButton);
  }

  /**
   * Method to approve subscription
   */
  async approveSubscription() {
    await this.click(this.approveButton);
    logger.info(`Successfully Approved subscription.`);
  }

  /**
   * Method to reject subscription
   */
  async rejectSubscription() {
    await this.click(this.rejectButton);
    logger.info(`Successfully Reject subscription.`);
  }

}

module.exports = { AddProductPage };
