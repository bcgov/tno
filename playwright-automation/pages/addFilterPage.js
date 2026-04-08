const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class AddFilterPage extends BasePage {
  constructor(page) {
    super(page);

    this.addNewFilterButton = page.getByRole('button', {  name : 'Add new filter'});
    this.backToFiltersButton = page.getByRole('button', { name: 'Back to filters' });
    this.nameInput = page.locator('#txt-name');
    this.descriptionInput = page.locator(`#txa-description`);
    this.keywordInputBox = page.locator(`textarea[id='txa-settings.search']`);

    this.filterLink = page.locator(`.ellipsis`);
    this.searchFilterInput = page.getByPlaceholder('Search by keyword');

    this.saveButton = page.getByRole('button', { name: 'Save' });

    this.deleteButton = page.getByRole('button', { name: 'Delete' });
    this.deleteConfirmation = page.locator('//button/div[text()="Yes, Remove It"]');

    this.toastNotification = page.locator('.Toastify .Toastify__toast-body div:nth-child(2)');

  }

  /**
   * Method to click on add new filter button.
   */
  async addNewFilter() {
    await this.click(this.addNewFilterButton);
    logger.info(`Clicked on Add new Filter button`);
  }

  /**
   * Method to check visibility of back to filter button.
   */
  async verifyBackToFilterVisibility() {
    logger.info(`Visibility of button is ${await this.isElementVisible(this.backToFiltersButton)}`);
    return await this.isElementVisible(this.backToFiltersButton);
  }

  /**
   * Method to enter Filter detail
   * @param {string} name
   */
  async enterFilterDetails(name, description) {
    await this.type(this.nameInput, name);

    await this.type(this.descriptionInput, description)

    logger.info(`Added Filters Details.`);
  }

  /**
   * Method to add query keyword
   * @param {string} keyword 
   */
  async enterQueryKeyword(keyword) {
    await this.type(this.keywordInputBox, keyword);
    logger.info(`Added query keyword`);
  }

  /**
   * Method to add Filter description
   * @param {*} description 
   */
  async addFilterDescription(description) {
     await this.type(this.descriptionInput, description)

    logger.info(`Added Filter Description.`);
  }

  /**
   * Method to select given filter
   * @param {string} filterName 
   */
  async selectFilter(filterName) {
    await this.type(this.searchFilterInput, filterName);
    await this.page.keyboard.press('Enter');
    await this.hardWait(500);
    logger.info(`Total Filter shown on grid : ${(await this.filterLink.nth(1)).count()}`);
    await this.click(this.filterLink.first());
    logger.info(`Selected filter is : ${filterName}`);
  }

  /**
   * Method to save the Filter details.
   */
  async save(){
    await this.click(this.saveButton.first());
    await this.hardWait(2000);
    logger.info(`Clicked on Save Button`);
  }

  /**
   * Method to click on given Filter subtab
   * @param {string} subTab Name
   */
  async clickOnFilterSubTab(tabName) {
    await this.click(
      this.page.locator(`//div[contains(@class,'tab-menu')]//span[text()="${tabName}"]`),
    );
    logger.info(`Clicked on SubTab : ${tabName}`);
  }


  /**
   * Method to go back to Filter grid
   */
  async backToFilterGrid() {
    await this.click(this.backToFiltersButton);
    logger.info(`Clicked on Back to Filters button`);
  }

  /**
   * Method to check visibility of Filter on the grid
   * @param {string} filterName 
   * @returns true if visible
   */
  async isAddedFilterVisibleOnGrid(filterName) {
    logger.info(`Is Added Filter Visible : ${await this.isTextPresentInCollection(this.filterLink, filterName)}`);
    return this.isTextPresentInCollection(this.filterLink, filterName);
  }

  /**
   * Method to select given filter
   * @param {string} filterName 
   */
  async searchFilter(filterName) {
    await this.type(this.filterInputTextBox, filterName);
    await this.page.keyboard.press('Enter');
    await this.hardWait(500);
    logger.info(`Selected filter : ${filterName}`);
  }

  /**
   * Method to delete the Filter
   */
  async deleteFilter() {
    await this.click(this.deleteButton.first());
    await this.click(this.deleteConfirmation);
    logger.info(`Successfully Deleted the newly added filter.`);
  }

  /**
   * Visiility of Success Toast message
   * @returns true if present else false
   */
  async isSuccessToastNotificationDisplayed(){
    logger.info(`Is Success Toast visible : ${ await this.isElementVisible(this.toastNotification)}`);
    return await this.isElementVisible(this.toastNotification);
  }

}

module.exports = { AddFilterPage };
