const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const TabManager = require('../utils/tab-Manager');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');
const { expect } = require ('@playwright/test');
const { time } = require('node:console');

class DataImport extends BasePage {
  constructor(page) {
    super(page);
    this.page = page;

this.dataImport = page.getByRole('button', { name: 'Data Import' })
this.ingestlink = page.getByRole('link', { name: 'Ingest Types' })
this.addNewIngest = page.getByRole('button', { name: 'plus Add New Ingest Type' })
this.ingestNameInput = page.getByRole('textbox', { name: 'Name' })
this.ingestDescriptionInput =  page.getByRole('textbox', { name: 'Description' })

this.ingestTypeDropdown = page.locator('.rs__indicator.rs__dropdown-indicator');

this.list = (value) => 
  page.getByText(value, { exact : true })

this.ingestSortOrderInput = page.getByRole('spinbutton', { name: 'Sort Order' })
this.saveButton = page.getByRole('button', { name: 'Save' })
this.backToIngestPage = page.getByRole('button', { name: 'back Back to Ingest Types' })
this.searchKeywordIngest = page.getByRole('textbox', { name: 'Search by keyword' });
this.selectRow = page.getByText('Automation Test Data');
this.dltbtn = page.getByRole('button', { name: 'Delete' });
this.remove = page.locator('button').filter({ hasText: 'Yes, Remove It' });

// Locators of Media Licenses
this.medialink = page.getByRole('link', { name: 'Media Licenses' })
this.addNewMedia = page.getByRole('button' , { name: 'plus Add new licence' })
this.mediaTTLInput = page.getByRole('spinbutton', { name: 'Time to Live (days)' })
this.backtoMedia = page.getByRole('button', { name: 'back Back to Licences' })
this.selectRowValue = page.getByText('Automation Test Media Licenses');
  
//LOcators Of Data Location 
this.dataLoctnlink = page.getByRole('link', { name: 'Data Locations' })
this.addNewDataLctn = page.getByRole('button', { name: 'plus Add new data location' })
this.DataTypeDropdown = page.locator('.rs__input-container');
this.listitm = (dvalue) => 
page.getByText(dvalue, { exact : true })
this.backtoDataLctn = page.getByRole('button', { name: 'back Back to DataLocations' })
this.rowValueSelected = page.getByText('Automation Test Data Locations');

// Locators of data Connections
this.dataConnctnlink = page.getByRole('link', { name: 'Data Connections' })
this.addNewDataConnctn = page.getByRole('button', { name: 'plus Add new connection' })
this.dataCnnctnDropdown = page.locator('.rs__input-container');
this.option = (dcvalue) => page.locator(`.rs__option:has-text("${dcvalue}")`);
this.dropDownPath = page.locator(`//*[@id="txt-configuration.path"]`)
this.nasPathInput = page.getByRole('textbox', { name: 'Path' });
this.pathfield = page.getByLabel('Path');
this.backtoDataConnt = page.getByRole('button', { name: 'back Back to Connections'})
this.searchDataCntn  = page.getByText('Automation Test Data Connections');
this.test = page.locator("//*[@id='root']/div[1]/div[2]/main/div/div[3]/div/div/div/div[1]");
this.deletebutton = page.locator(`//*[@id="root"]/div[1]/div[2]/main/div/div/div/form/div/div[2]/button[2]`);
}

/**methods of Add Ingest Type****/
async navigateToDataImport() {
  await this.dataImport.click();
  logger.info(`Clicked on Data Import button!!`);
}
async navigateToIngest() {
  await this.ingestlink.click();
  logger.info(`Clicked on Ingest Type in Menue`);
}
async clickAddNewIngest() {
  await this.addNewIngest.click();
  logger.info(`Clicked on Add New Ingest TYpe button!!`);
}
async enterIngestDetails(name, description, sortOrder) {
  await this.ingestNameInput.type(name);
  await this.ingestDescriptionInput.type(description);
  await this.ingestSortOrderInput.type(sortOrder.toString());

  logger.info(`Enter Tag: Name=${name}, Description=${description}, Sort Order=${sortOrder}`);

}
async dropdownOption(value) {
  await this.ingestTypeDropdown.click();
  await this.list(value).click();
  await this.saveButton.click();

  console.log("Listoption :", value);
}
async backbtnIngest() {
 await this.backToIngestPage.click();
  logger.info(`Clicked on Back to Ingest Type button!!`);
}
async searchKeyword(ingestName){
//search
  await this.searchKeywordIngest.click();
  await this.searchKeywordIngest.fill(ingestName);
  await this.searchKeywordIngest.press('Enter');

  await this.page.waitForLoadState('networkidle');
  //locate row
   const row = this.page.getByText(ingestName);
   logger.info(`This line to be printed: Name=${row}`);
}
async clickonRow(){
await this.selectRow.click();
logger.info("Click on Selected Value");
}
async clickDelete(){
await this.dltbtn.click();
logger.info("Delete the Record");
}
async removeBtn(){
await this.remove.click();
logger.info("Remove data and confirm");
}
/*** Methods of Media type */  
async navigatetoMediaL(){
  await this.medialink.click();
  logger.info("Click on Media Licenses in Drop down");
  }
async clickAddNewLicense() {
  await this.addNewMedia.click();
  logger.info(`Clicked on Add New Media Licenses button!!`);
}
async enterMediaLicensetDetails(name, description, sortOrder, ttl) {
  await this.ingestNameInput.type(name);
  await this.ingestDescriptionInput.type(description);

  await this.ingestSortOrderInput.type(sortOrder.toString());
  await this.mediaTTLInput.type(ttl.toString());

  logger.info(`Enter Media Tag: Name=${name}, Description=${description}, Sort Order=${sortOrder}, Time to Live=${ttl}`);
   
  await this.saveButton.click();
  logger.info('Media Licenses Information Saved');

  await this.backtoMedia.click()
  logger.info('Navigate back to Media licenses')

}
async searchbox(mediaName){
//search
  await this.searchKeywordIngest.click();
  await this.searchKeywordIngest.fill(mediaName);
  await this.searchKeywordIngest.press('Enter');

  await this.page.waitForLoadState('networkidle');
  //locate row
   const row = this.page.getByText(mediaName);
   logger.info(`This line to be printed: Name=${row}`);

   await this.selectRowValue.click();
   logger.info("Click on Selected Value");
}
/** Methods of Data locations  */
async navigatetoDataLoctn(){
  await this.dataLoctnlink.click();
  logger.info("Click on Data Location in Drop down");
  }
async clickAddNewDataLoctn() {
  await this.addNewDataLctn.click();
  logger.info(`Clicked on Add New Data Location button!!`);
}
async dropdownValue(dvalue) {
  await this.DataTypeDropdown.click();
  logger.info("Clicked on Data Location Drop Down");
  await this.listitm(dvalue).click();
  logger.info(`Selected Data Location Type: ${dvalue}`);
  await this.saveButton.click();
  logger.info('Data Location Information Saved');
  console.log("Listoption :", dvalue);
  await this.backtoDataLctn.click()
  logger.info('Navigate back to Data Location Page')
}
async Search(dataName){
  await this.searchKeywordIngest.click();
  await this.searchKeywordIngest.fill(dataName);
  await this.searchKeywordIngest.press('Enter');

  await this.page.waitForLoadState('networkidle');
  //locate row
   const row = this.page.getByText(dataName);
   logger.info(`This line to be printed: Name=${row}`);

   await this.rowValueSelected.click();
   logger.info("Click on Selected Value");
}
/** Methods For Data Connections */
async navigateToDataCnctn() {
  await this.dataConnctnlink.click();
  logger.info(`Clicked on Data Connctions in Menue`);
}
async clickAddNewDataCntn() {
  await this.addNewDataConnctn.click();
  logger.info(`Clicked on Add New Data Connections button!!`);
}
async dropdownfield(dcvalue){

await this.dataCnnctnDropdown.click();
logger.info("Clicked on Data Connection Drop Down");

await this.page.keyboard.type(dcvalue);  

const option = this.page.locator(`.rs__option:has-text("${dcvalue}")`, { exact : true });
await option.waitFor({ state: 'visible' });
await option.click();
logger.info(`Selected Data Connection Type: ${dcvalue}`); 

//path
await this.nasPathInput.click();
await this.nasPathInput.fill('Path Test Data');
logger.info('Enter path for NAS');
await this.saveButton.click();
logger.info('Click on Save button');
}
async backbtndataC() {
  logger.info('enter into backbut function');
  await this.backtoDataConnt.click();
//await this.page.locator(`//*[@id="root"]/div[1]/div[2]/main/div/button`).click();
  logger.info(`Clicked on Back to Ingest Type button!!`);
}
async searchboxValue(data){
//search
  await this.searchKeywordIngest.click();
  await this.searchKeywordIngest.fill(data);
  await this.searchKeywordIngest.press('Enter');

  await this.page.waitForLoadState('networkidle');
  //locate row
   const rownew = this.page.getByText(data);
   logger.info(`This line to be printed: Name=${rownew}`);
}

async clickonrow(){
this.searchDataCntn = this.page.locator('//*[@id="root"]/div[1]/div[2]/main/div/div[3]/div/div/div/div[1]').first();
//this.searchDataCntn = this.page.getByText('Automation Test Data Connections');
await this.searchDataCntn.waitFor({ state: 'visible' });
const text = await this.searchDataCntn.textContent();
 logger.info(`Row Vaue : ${text}`);
 await this.searchDataCntn.click();
 logger.info(`Click on selected row`)
}
}

module.exports = { DataImport }

