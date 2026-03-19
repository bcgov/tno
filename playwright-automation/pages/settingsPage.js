const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { getFilePath } = require('../utils/fileUpload.util');
const { expect } = require('@playwright/test');

class SettingsPage extends BasePage {
  constructor(page) {
    super(page);

    this.advancedSearchLink = page.getByRole('link', { name: 'Go Advanced' });
    this.advancedSearchTextArea = page.locator('textarea')
    this.advancedSearchInput = page.locator('input[name="searchName"]')
    this.saveSearchButton = page.getByRole('button', { name: 'Save' });
    this.mySavedSearchLink = page.getByText('My Saved Searches');
    this.clearSearchInput = page.getByRole('textbox');
  }


  async clickOnAdvancedSearchLink() {
    await this.click(this.advancedSearchLink);
    logger.info('Clicked on Advanced Search link!!');
  }

  async enterAdvancedSearchText(searchText) {
    await this.type(this.advancedSearchTextArea, searchText);
    logger.info(`Entered advance search text as ${searchText}!!`);
  }

    async enterAdvancedSearchName(searchName) {     
    await this.type(this.advancedSearchInput, searchName);
    logger.info(`Entered advance search name as ${searchName}!!`);
  }
  async clickOnSaveSearchButton() {
    await this.click(this.saveSearchButton,{ force: true });
    logger.info('Clicked on Save Search button!!');
  }

  async clickOnSavedSearchesLink() {
    await this.click(this.mySavedSearchLink);
    logger.info('Clicked on My Saved Search link!!');
  }

  async clearSearchInputText(){
    await this.clear(this.clearSearchInput);
    logger.info('Cleared search input text!!');
  }

}
module.exports = { SettingsPage };
