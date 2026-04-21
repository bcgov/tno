const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const TabManager = require('../utils/tab-Manager');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');
const { expect } = require ('@playwright/test');

class SystemSettings extends BasePage {
  constructor(page) {
    super(page);
    this.page = page;

this.systembtn = page.getByRole('button', { name: 'System Settings' });
this.systemConfigbtn = page.getByRole('link', { name: 'System Configuration' });
this.addnewsettings = page.getByRole('button', { name: 'plus Add new setting' });
this.addname = page.getByRole('textbox', { name: 'Name' });
this.adddecsc = page.getByRole('textbox', { name: 'Description' });
this.addValue = page.getByRole('textbox', { name: 'Value' });
this.addSortOrder = page.getByRole('spinbutton', { name: 'Sort Order' });
this.savebtn = page.getByRole('button', { name: 'Save' });
this.alertlert = page.getByRole('alert');
this.backBtn = page.getByRole('button', { name: 'back Back to settings' });
this.searchfiled = page.getByRole('textbox', { name: 'Search by keyword' });
this.fieldValue = page.getByText('Automation Test System');
this.btndelete = page.getByRole('button', { name: 'Delete' });
this.btnremove = page.locator('button').filter({ hasText: 'Yes, Remove It' });
 
//Work Order
this.workorderbtn = page.getByRole('link', { name: 'Work Orders' });
this.searchfield = page.locator('.rs__input-container').first();
this.searchfieldValue = page.locator('#react-select-2-input');
this.selectoption = page.getByRole('option', { name: 'Transcription' });
this.searchbtn = page.getByRole('button', { name: 'search' });
this.tablerow = page.getByText('Transcription').nth(2);
this.savebtn = page.getByRole('button', { name: 'Save' });
this.alertvalue = page.getByText('Work order has successfully');
this.backbutn = page.getByRole('button', { name: 'back Back to WorkOrders' });

this.searchbystatus = page.locator('#sel-status > .rs__control > .rs__value-container > .rs__input-container');
this.entervalue = page.locator('#react-select-23-input');
this.selectoption2 = page.getByRole('option', { name: 'Completed' });
}
async navigateToSystemSettings() {
  await this.systembtn.click();
  logger.info('Clicked on System Settings button');
}
async navigatetoSystemConfig() {
  await this.systemConfigbtn.click();
  logger.info('Clicked on System Configuration button');
}
async clickAddNewSystemConfig() {
  await this.addnewsettings.click();
  logger.info('Clicked on Add New Setting button');
}
async enterSystemConfigDetails(name, description, value, sortOrder) {
  await this.addname.fill(name);
  logger.info(`Entered Name: ${name}`); 
  await this.adddecsc.fill(description);
  logger.info(`Entered Description: ${description}`);
  await this.addValue.fill(value);
  logger.info(`Entered Value: ${value}`);
  await this.addSortOrder.fill(sortOrder);
  logger.info(`Entered Sort Order: ${sortOrder}`);
  await this.savebtn.click();
  logger.info('Clicked on Save button');
  await expect(this.alertlert).toHaveText('Automation Test System Configuration has successfully been saved.');
  logger.info('Verified success alert for saving system configuration');
}
  async navigateBackToSettings() {
    await this.backBtn.click();
    logger.info('Clicked on Back to Settings button');
  }
async searchSystemConfig(keyword) {
  await this.searchfiled.click();
  logger.info('Selected the system configuration from search results');
  await this.searchfiled.fill(keyword);
  logger.info(`Entered search keyword: ${keyword}`);
  await this.searchfiled.press('Enter');
  await this.page.waitForLoadState('networkidle');
  //locate row
   const rowValue = this.page.getByText(keyword);
   logger.info(`This line to be printed: Name=${rowValue}`);
   await this.fieldValue.click();
   logger.info("Click on Selected Value");
} 
async deleteSystemConfig() {
  await this.btndelete.click();
  logger.info('Clicked on Delete button');  
}
async confirmDelete() {
  await this.btnremove.click();
  logger.info('Confirmed deletion of system configuration');
}  
async navigatetoWorkOrder() {
  await this.workorderbtn.click();
  logger.info('Clicked on Work Orders button');
}
async searchWorkOrder(keyword) {
  await this.searchfield.click();
  logger.info('Clicked on Search field');
}
async enterSearchKeyword(keyword) {
  await this.page.keyboard.type(keyword);
  logger.info(`Entered search keyword: ${keyword}`);
  await this.selectoption.click();
  logger.info(`Selected option: ${keyword}`);
  await this.searchbtn.click();
  logger.info('Clicked on Search button');
  await this.page.waitForLoadState('networkidle');
  const rowValue = this.tablerow;
  await this.tablerow.click();
  logger.info(`This line to be printed: Name=${rowValue}`);
}
async SaveWorkOrder() {
  await this.savebtn.click();
  logger.info('Clicked on Save button');
  await expect(this.alertvalue).toHaveText('Work order has successfully been saved.');
  logger.info('Verified success alert for saving work order');
}
async backtoWorkorder() {
  await this.backbutn.click();
  logger.info('Clicked on Back to WorkOrders button');  
}
async searchWorkOrderbystatus(keyword) {
  await this.searchbystatus.click();
  logger.info('Clicked on Search by Status field');
}
async enterstatuskeyword(value) {
  await this.page.keyboard.type(value);
  logger.info(`Entered search keyword: ${value}`);
  await this.selectoption2.click();
  logger.info(`Selected option: ${value}`);
  await this.searchbtn.click();
  logger.info('Clicked on Search button');
  await this.page.waitForLoadState('networkidle');
}
  async tablerowfiels() {
  const rowValue2 = this.tablerow;
  await this.tablerow.click();
  logger.info(`This line to be printed: Name=${rowValue2}`);
}
 async verifyAllStatusCompleted() {
   logger.info('Enter verifyAllStatusCompleted method');
  const statusElements = this.page.locator('.grid-column .ellipsis'); // Adjust the selector to target the correct column
 
    const statusText = await statusElements.nth(5).textContent();
    logger.info(`Status Text: ${statusText}`);
    expect(statusText.trim()).toBe('Completed');
  }
async NextPage() {
    const nextButton = this.page.getByRole('img', { name: 'next' });
    await nextButton.click();
    logger.info('Clicked on Next page button');

    // Fetch page number "2"
    const text = this.page.getByText('2', { exact: true });
    const visibletext = await text.textContent();
    logger.info(`Visible Text: ${visibletext}`);

    await this.page.waitForLoadState('networkidle');

    // Fetch "Page 2 of"
    const pagenumber = this.page.getByText('Page 2 of');
    const visibletext2 = await pagenumber.textContent();
    logger.info(`Visible Text Second : ${visibletext2}`);

    // Extract only the current page number
const currentPage = visibletext2.split(' ')[1];        // "2"
logger.info(`Current Page: ${currentPage}`);

if (visibletext === currentPage) {
    logger.info("Both are SAME");
} else {
    logger.info("Both are DIFFERENT");
}
}


}
module.exports = { SystemSettings }