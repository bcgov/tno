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
    this.settingsLink = page.getByText('Settings')
    this.myAccountLink = page.getByText('My Account')
    this.emailToggleButton = page.locator('[class="sc-dhKdPU cXQtTG button-toggle"]').nth(0);
    this.reportsentimentToggleButton = page.locator('[class="sc-dhKdPU cXQtTG button-toggle"]').nth(1);
<<<<<<< HEAD
    this.myColleaguesLink = page.getByText('My Colleagues')
    this.addColleagueEmailInput = page.locator('input[name="email"]')
    this.addColleagueButton = page.getByRole('button', { name: 'Add' })
this.removeColleagueButton = page.locator('[class="sc-eyvHYj kJjhiH action"]').nth(0);
this.confirmRemoveColleagueButton = page.getByText('Yes, Remove It')

  }

  async clickOnRemoveColleagueButton() {
    await this.click(this.removeColleagueButton);
    await this.click(this.confirmRemoveColleagueButton);
    logger.info('Clicked on Remove Colleague button!!');
  } 

async clickOnMyColleaguesLink() {
    await this.click(this.myColleaguesLink);
    logger.info('Clicked on My Colleagues link!!');
  }
async clickOnAddColleagueButton() {
    await this.click(this.addColleagueButton);
    logger.info('Clicked on Add Colleague button!!');
  }

  async enterColleagueEmail(email) {
    await this.type(this.addColleagueEmailInput, email);
    logger.info(`Entered colleague email as ${email}!!`);
  } 


=======
  }

>>>>>>> dev
async toggleReportSentimentOn() {
    const isChecked = await this.reportsentimentToggleButton.getAttribute('value');
    console.log('isChecked value:', isChecked); // Debug log to check the value
    if (isChecked === 'false') {
             await this.reportsentimentToggleButton.click();
      expect(await this.reportsentimentToggleButton.getAttribute('value')).toBe('true');
      logger.info(`Report sentiment is already ON`);

    } else {
            logger.info(`Report sentiment Already toggled ON`);
            expect(await this.reportsentimentToggleButton.getAttribute('value')).toBe('true');

    }
  }

async toggleEmailNotificationOn() {
    const isChecked = await this.emailToggleButton.getAttribute('value');
    console.log('isChecked value:', isChecked); // Debug log to check the value
    if (isChecked === 'false') {
             await this.emailToggleButton.click();
      logger.info(`Email notification is already ON`);
      expect(await this.emailToggleButton.getAttribute('value')).toBe('true');

    } else {
            logger.info(`Email notification Already toggled ON`);
            expect(await this.emailToggleButton.getAttribute('value')).toBe('true');

    }
  } 

async toggleReportSentimentOFF() {
    const isChecked = await this.reportsentimentToggleButton.getAttribute('value');
    console.log('isChecked value:', isChecked); // Debug log to check the value
    if (isChecked === 'false') {
      expect(await this.reportsentimentToggleButton.getAttribute('value')).toBe('false');
      logger.info(`Report sentiment is already OFF`);

    } else {
            await this.reportsentimentToggleButton.click();
            logger.info(`Report sentiment Already toggled OFF`);
            expect(await this.reportsentimentToggleButton.getAttribute('value')).toBe('false');

    }
  }

async toggleEmailNotificationOFF() {
    const isChecked = await this.emailToggleButton.getAttribute('value');
    console.log('isChecked value:', isChecked); // Debug log to check the value
    if (isChecked === 'false') {
      logger.info(`Email notification is already OFF`);
      expect(await this.emailToggleButton.getAttribute('value')).toBe('false');

    } else {
                   await this.emailToggleButton.click();

            logger.info(`Email notification Already toggled OFF`);
            expect(await this.emailToggleButton.getAttribute('value')).toBe('false');

    }
  } 



  async clickOnMyAccountLink() {
    await this.click(this.myAccountLink);
    logger.info('Clicked on My Account link!!');
  }
async clickOnSettingsLink() {
    await this.click(this.settingsLink);
    logger.info('Clicked on Settings link!!');
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
