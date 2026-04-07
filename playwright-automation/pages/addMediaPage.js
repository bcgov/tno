const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const TabManager = require('../utils/tab-Manager');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');
const { expect } = require ('@playwright/test');

class AddMediaPage extends BasePage {
  constructor(page) {
    super(page);
    this.page = page;

this.contentConfigurationButton = page.getByRole('button', { name: 'Content Configuration' })
this.mediasLink = page.getByRole('link', { name: 'Media Types' })
this.addNewmediaButton = page.getByRole('button', { name: 'plus Add new media type' })
this.mediaNameInput = page.getByRole('textbox', { name: 'Name' })
this.mediaDescriptionInput =  page.getByRole('textbox', { name: 'Description' })
this.mediaPositionInput = page.locator('.rs__input-container')
this.mediaTypeDropdown = page.locator('label:has-text("List Option")').locator('..').locator('.rs__input-container');

this.listOption = (value) => 
  page.getByText(value, { exact : true })
this.mediaSortOrderInput = page.getByRole('spinbutton', { name: 'Sort Order' })
this.saveButton = page.getByRole('button', { name: 'Save' })

this.searchInput = page.getByRole('textbox', { name: 'Search by keyword' });

this.selectTRvalue = page.getByText('Automation Media type Name');
this.deletebtn = page.getByRole('button', { name: 'Delete' });
this.removebtn = page.locator('button').filter({ hasText: 'Yes, Remove It' });
console.log(page.getByRole('button', { name: 'Delete' }).count());

// Tags Page Locators
this.tagLink = page.getByRole('link', { name: 'Tags' });
this.addnewtag = page.getByRole('button', { name: 'plus Add new tag' })
this.tagCodeInput = page.locator(`//*[@id="txt-code"]`)
this.tagNameInput = page.locator(`//*[@id="txt-name"]`)
this.tagDescriptionInput = page.locator(`//*[@id="txa-description"]`)
this.tagSortOrderInput = page.locator(`//*[@id="txt-sortOrder"]`)
this.backToTagButton = page.getByRole('button', { name: 'back Back to Tags' })
this.selectTagvalue = page.getByText('AGG Auto');
this.searchByCodeInput = page.getByRole('textbox', { name: 'Search by keyword' })

//Pundits Page Locators

this.punditLink = page.getByRole('link', { name: 'Columnist & Pundits' });
this.addnewpundit = page.getByRole('button', { name: 'plus Add new columnist/pundit' })
this.punditdropdown = page.locator('.rs__input-container')
this.lstOption = (value) => 
  page.getByText(value, { exact : true })
this.selectpunditvalue = page.getByText('Test Automation Pundit');
this.backTopunditButton = page.getByRole('button', { name: 'back Back to Columns/Pundits' })
this.searchByCodeInput = page.getByRole('textbox', { name: 'Search by keyword' })

// Ingest Type -Data Import module 
this.ingestModule = page.getByRole('button', { name: 'Data Import' });
this.ingestLink = page.getByRole('link', { name: 'Ingest Types' });
this.addnewIngest = page.getByRole('button', { name: 'plus Add New Ingest Type' });

this.ingestDropdown = page.locator('svg').nth(4)
this.listOption2 = (value) => 
  page.getByText(value, { exact : true })
this.selectIngestvalue = page.getByText('Test Automation IT');
this.backToIngestButton = page.getByRole('button', { name: 'back Back to Ingest Types' });

  }
 

/**methods of Add Media Scenario****/

async navigateToCC() {
  await this.contentConfigurationButton.click();
  logger.info(`Clicked on Content Configuration button!!`);
}

async navigateToMiMedia() {
  await this.mediasLink.click();
  logger.info(`Clicked on Media in Menue`);
}

async clickAddNewMedia() {
  await this.addNewmediaButton.click();
  logger.info(`Clicked on Add New Media button!!`);
}

async enterMediaDetails(name, description, sortOrder) {
  await this.mediaNameInput.type(name);
  await this.mediaDescriptionInput.type(description);

  await this.mediaSortOrderInput.type(sortOrder.toString());

  logger.info(`Enter Media Tag: Name=${name}, Description=${description}, Sort Order=${sortOrder}`);

}

async selectListTypeOption(value) {
  await this.mediaTypeDropdown.click();
  await this.listOption(value).click();
  await this.saveButton.click();

  console.log("Listoption :", value);
}

/**************Search and Delete Media Method**********/

async searchAndValidation(mediaName){
//search
  await this.searchInput.click();
  await this.searchInput.fill(mediaName);
  await this.searchInput.press('Enter');

  await this.page.waitForLoadState('networkidle');
  //locate row
   const row = this.page.getByText(mediaName);
   logger.info(`This line to be printed: Name=${row}`);
}

async clickOnVisibleText(){
await this.selectTRvalue.click();
logger.info("Click on Selected Value");
}

async clickOnDelete(){
await this.deletebtn.click();
logger.info("Delete the Record");
}

async removeData(){
await this.removebtn.click();
logger.info("Remove data and confirm");
}

/*************Tags Page methods************/

async navigateToTAGS() {
  await this.tagLink.click();
  logger.info(`Clicked on Tags in Menue`);
}

async clickAddNewTag() {
  await this.addnewtag.click();
  logger.info(`Add New Tags`);
}

async enterTagDetails(code, tname, tdescription, tsortorder) {
  await this.tagCodeInput.type(code);
  await this.tagNameInput.type(tname);
  await this.tagDescriptionInput.type(tdescription);
  await this.tagSortOrderInput.type(tsortorder.toString());
  await this.saveButton.click();
 
  logger.info(`Enter Media Tag: Code= ${code}, Name=${tname}, Description=${tdescription}, Sort Order=${tsortorder}`);
  logger.info(`Save Data!!`);
}

async clickBackToTAG() {
  await this.backToTagButton.click({ force: true });
  logger.info(`Clicked on Back to Tags button!!`);
}

async searchTagByName(name) {
  await this.searchByCodeInput.type(name);
  logger.info(`Searched for TAG with name: ${name}`);
}

async searchTagValue(TagCode){
//search
  await this.searchInput.click();
  await this.searchInput.fill(TagCode);
  await this.searchInput.press('Enter');

  await this.page.waitForLoadState('networkidle');
  //locate row
   const row = this.page.getByText(TagCode);
   logger.info(`This line to be printed: Name=${row}`);
}

async clickOnVisiblecode(){
await this.selectTagvalue.click();
logger.info("Click on Selected TAG Value");
}

/***** Pundit page methods */

async navigateToColumn_Pundit() {
  await this.punditLink.click();
  logger.info(`Clicked on Column/Pundit in Menue`);
}
async clickAddNewPundit() {
  await this.addnewpundit.click();
  logger.info(`Clicked on Column/Pundit in Menue`);
}

async clickBackToColumn_pundit() {
  await this.backTopunditButton.click({ force: true });
  logger.info(`Clicked on Back to Column/Pundit button!!`);
}

async selectListOptValue(value) {
  await this.punditdropdown.click();
  await this.lstOption(value).click();
  await this.saveButton.click();

  console.log("Listoption :", value);
}

async clickOnVisibledata(){
await this.selectpunditvalue.click();
logger.info("Click on Selected Pundit Value");
}

/*************   Methods for Ingestion Type -Data Import  */
async navigateToDataImport() {
  await this.ingestModule.click();
  logger.info(`Clicked on Ingest Type in Menue`);
}

async navigateToIngestType() {
  await this.ingestLink.click();
  logger.info(`Clicked on Ingest Type in Menue`);
}
async clickAddNewIngestType() {
  await this.addnewIngest.click();
  logger.info(`Clicked on Ingest Type in Menue`);
}

async clickBackToIngestType() {
  await this.backToIngestButton.click({ force: true });
  logger.info(`Clicked on Back to Ingest button!!`);
}

async selectListOption(value) {
  await this.ingestDropdown.click();
  await this.listOption2(value).click();
  await this.saveButton.click();

  console.log("Listoption :", value);
}
async clickOnSearchValue(){
await this.selectIngestvalue.click();
logger.info("Click on Selected Ingest Value");

}
}
module.exports = { AddMediaPage };
