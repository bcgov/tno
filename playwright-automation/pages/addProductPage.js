const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class AddProductPage extends BasePage {
  constructor(page) {
    super(page);

    this.backToProductButton = page.getByRole('button', { name: 'Back to Products' });
    this.nameInput = page.getByPlaceholder('Enter unique product name');
    this.productTypeDropdown = page.locator(`input[name="select-productType"]`);
    this.targetProductDropdown = page.locator(`input[name="targetProduct"]`);
    this.saveButton = page.getByRole('button', { name: 'Save' });

    this.deleteButton = page.getByRole('button', { name: 'Delete' });
    this.deleteConfirmation = page.locator('//button/div[text()="Yes, Remove It"]');

    this.searchSubscriberInput = page.getByPlaceholder('Search by keyword');
    this.searchSubscriberButton = page.locator(`//button//*[@alt="search"]`);
    this.subscriberRecordCheckbox = page.locator(`.grid-column  input[type="checkbox"]`);

    this.productLink = page.locator(`.link`);

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

    await this.type(this.productTypeDropdown, productType);
    await this.page.keyboard.press('Tab');

    await this.type(this.targetProductDropdown, targetProduct);
    await this.page.keyboard.press('Tab');

    logger.info(`Added Product Details.`);
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
    logger.info(`Is Added product Visible : ${await this.isTextPresentInCollection(this.productLink, productName)}`);
    return this.isTextPresentInCollection(this.productLink, productName);
  }

  /**
   * Method to select given produc
   * @param {string} productName 
   */
  async selectProduct(productName) {
    await this.type(this.searchSubscriberInput, productName);
    await this.page.keyboard.press('Enter');
    await this.hardWait(500);
    logger.info(`Total product shown on grid : ${await this.productLink.count()}`);
    await this.click(this.productLink);
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

}

module.exports = { AddProductPage };
