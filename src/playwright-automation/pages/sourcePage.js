const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { getFilePath } = require('../utils/fileUpload.util');
const { expect } = require('@playwright/test');

class SourcePage extends BasePage {
  constructor(page) {
    super(page);
 this.addSourceButton = page.getByRole('button', { name: 'plus Add New Source' })
 this.legalNameInput = page. getByRole('textbox', { name: 'Legal Name *' })
 this.codeInput = page.getByRole('textbox', { name: 'Code *' })
 this.licenseDropDown = page.locator('.rs__input-container').first()
 this.saveButton = page. getByRole('button', { name: 'Save' })
 this.backToSourcesButton = page. getByRole('button', { name: 'back Back to Sources' })
 this.enabledCheckbox = page.getByRole('checkbox', { name: 'Enabled' })
 this.useInTopicsCheckbox = page. getByRole('checkbox', { name: 'Use in Topics' })
 this.disableTranscriptRequestsCheckbox = page. getByRole('checkbox', { name: 'Disable transcript requests' })
this.searchInput = page. getByRole('textbox', { name: 'Search by keyword' })

  }




  async clickOnAddSourceButton() {
    await this.click(this.addSourceButton);
    logger.info('Clicked on Add Source button!!');
  }


  async enterLegalName(legalName) {
    await this.type(this.legalNameInput, legalName);
    logger.info(`Entered legal name: ${legalName}`);
  }

  async enterCode(code) {
    await this.type(this.codeInput, code);
    logger.info(`Entered code: ${code}`);
  }

  async selectLicense(license) {
    await this.click(this.licenseDropDown);
    const option = this.page.locator('.rs__option').filter({ hasText: license });
    await this.click(option);
    logger.info(`Selected license: ${license}`);
  }

  async clickOnSaveButton() {
    await this.click(this.saveButton);
    logger.info('Clicked on Save button!!');
  }
  async clickOnBackToSourcesButton() {
    await this.click(this.backToSourcesButton);
    logger.info('Clicked on Back to Sources button!!');
  }
  async toggleEnabledCheckbox() {
    await this.click(this.enabledCheckbox);
    logger.info('Toggled Enabled checkbox!!');
  }
    async toggleUseInTopicsCheckbox() {
    await this.click(this.useInTopicsCheckbox);
    logger.info('Toggled Use in Topics checkbox!!');
  }
    async toggleDisableTranscriptRequestsCheckbox() {
    await this.click(this.disableTranscriptRequestsCheckbox);
    logger.info('Toggled Disable transcript requests checkbox!!');  
    }

async searchLeagelName(legalName) {
    await this.type(this.searchInput, legalName);
    logger.info(`Searched for legal name: ${legalName}`);
  } 
  
}
module.exports = { SourcePage };
