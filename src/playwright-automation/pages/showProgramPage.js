const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { getFilePath } = require('../utils/fileUpload.util');
const { expect } = require('@playwright/test');

class ShowProgramPage extends BasePage {
  constructor(page) {
    super(page);

    this.addShowProgramButton = page.getByRole('button', { name: 'plus Add new show/program' })
    this.nameInput = page.getByRole('textbox', { name: 'Name' })
    this.sourceDropDown = page.locator('.rs__input-container').first()
    this.descriptionInput = page.getByRole('textbox', { name: 'Description' })
    this.mediaTypeDropDown = page.locator('.rs__value-container.rs__value-container--is-multi > .rs__input-container')
    this.programSortOrderInput = page.getByRole('spinbutton', { name: 'Sort Order' })
    this.enabledCheckbox = page.getByRole('checkbox', { name: 'Is Enabled' })
    this.automaticallyTranscribeCheckbox = page.getByRole('checkbox', { name: 'Automatically transcribe when' })
    this.saveButton = page.getByRole('button', { name: 'Save' })
    this.backToShowProgramButton = page.getByRole('button', { name: 'back Back to Show/Programs' })
    this.searchInput = page.getByRole('textbox', { name: 'Search by keyword' })
    this.deleteButton = page.getByRole('button', { name: 'Delete' })
    this.confirmDeleteButton = page.locator('button').filter({ hasText: 'Yes, Remove It' })
  }

  async clickOnAddShowProgramButton() {
    await this.click(this.addShowProgramButton);
    logger.info('Clicked on Add Show/Program button!!');
  }

  async enterName(name) {
    await this.type(this.nameInput, name);
    logger.info(`Entered show/program name: ${name}`);
  }
async selectSource(source) {
    await this.click(this.sourceDropDown);
    const option = this.page.locator('.rs__option').filter({ hasText: source });
    await this.click(option);
    logger.info(`Entered source: ${source}`);
  }
  async enterDescription(description) {
    await this.type(this.descriptionInput, description);
    logger.info(`Entered description: ${description}`);
  }

  async selectMediaType(mediaType) {
    await this.click(this.mediaTypeDropDown);
    const option = this.page.locator('.rs__option').filter({ hasText: mediaType });
    await this.click(option);
    logger.info(`Selected media type: ${mediaType}`);
  }

  async enterProgramSortOrder(sortOrder) {
    await this.type(this.programSortOrderInput, sortOrder.toString());
    logger.info(`Entered program sort order: ${sortOrder}`);
  }

  async toggleEnabledCheckbox() {
    if(await this.enabledCheckbox.isChecked()) {
      logger.info('Enabled checkbox is already checked');
      return;
    }else {
    await this.click(this.enabledCheckbox);
    logger.info('Toggled Is Enabled checkbox!!');
    }
  } 

    async toggleAutomaticallyTranscribeCheckbox() {     
    if(await this.automaticallyTranscribeCheckbox.isChecked()) {
      logger.info('Automatically Transcribe checkbox is already checked');
      return;
    }else {
    await this.click(this.automaticallyTranscribeCheckbox);
    logger.info('Toggled Automatically Transcribe checkbox!!');
    } 
}
async clickOnSaveButton() {
    await this.click(this.saveButton);
    logger.info('Clicked on Save button!!');
}

async clickOnBackToShowProgramButton() {
    await this.click(this.backToShowProgramButton);
    logger.info('Clicked on Back to Show/Programs button!!');   
}
async searchShowProgramByName(name) {
    await this.type(this.searchInput, name);
    logger.info(`Searched for show/program name: ${name}`);
  }
  async clickProgramRecord(programName) {
    const programRecord = this.page.locator('div[class*="row"]').filter({ hasText: programName }).first();
    await this.click(programRecord);
    logger.info(`Clicked on show/program record: ${programName}`);
  }
  async deleteShowProgram() {
    await this.click(this.deleteButton);
    logger.info(`Clicked on Delete button for show/program`);
    await this.hardWait(3000);
    await expect(this.confirmDeleteButton).toBeVisible();
    await this.click(this.confirmDeleteButton);
    logger.info(`Confirmed deletion of show/program`);
  }
}

module.exports = { ShowProgramPage };
