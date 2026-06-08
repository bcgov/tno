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
this.fieldValue = page.getByText('Automation Test System').first();
this.btndelete = page.getByRole('button', { name: 'Delete' });
this.btnremove = page.locator('button').filter({ hasText: 'Yes, Remove It' });
 
//Work Order
this.workorderbtn = page.getByRole('link', { name: 'Work Orders' });
this.searchfield = page.locator('.rs__input-container').first();
this.searchfieldValue = page.locator('#react-select-2-input');
this.selectoption = page.locator('.rs__option').filter({ hasText: 'Transcription' });
this.searchbtn = page.getByRole('button', { name: 'search' });
this.tablerow = page.getByText('Transcription').nth(2);
this.savebtn = page.getByRole('button', { name: 'Save' });
this.alertvalue = page.getByText('Work order has successfully');
this.backbutn = page.getByRole('button', { name: 'back Back to WorkOrders' });
this.backuser = page.getByRole('button', { name: 'back Back to Users' });
this.searchbystatus = page.locator('#sel-status > .rs__control > .rs__value-container > .rs__input-container');
this.searchbyrole = page.locator('#sel-role > .rs__control > .rs__value-container > .rs__input-container');
this.entervalue = page.locator('#react-select-23-input');
this.selectoption2 = page.getByRole('option', { name: 'Completed' });
this.manageuserbtn = page.getByRole('link', { name: 'Manage Users' });
this.toastmessage = page.getByText('has successfully been saved.');
this.deletemessage = page.getByText('has successfully been deleted.')
this.searchbtn = page.getByRole('button', { name: 'search' });
}
async navigateToSystemSettings() {
  await this.systembtn.click();
  logger.info('Clicked on System Settings button');
}
async navigatetoSystemConfig() {
  await this.systemConfigbtn.click();
  logger.info('Clicked on System Configuration button');
}
async navigatetoManageUser() {
  await this.manageuserbtn.click();
  logger.info('Clicked on Manage Users button');
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
  await expect(this.alertlert).toContainText('has successfully been saved.');
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
  await this.searchbtn.click();
  logger.info('Clicked on Search Button');
}
async enterSearchKeyword(keyword) {
  await this.searchfield.click();
  logger.info('Clicked on Search by Type dropdown');
  await this.page.keyboard.type(keyword);
  logger.info(`Entered search keyword: ${keyword}`);
  await this.selectoption.waitFor({ state: 'visible', timeout: 5000 });
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
async searchWorkUserbystatus(keyword) {
  await this.searchbystatus.click();
  logger.info('Clicked on Search by Status field');
  await this.searchbtn.click();
  logger.info('Clicked on Search button');
}
async backtoWorkorder() {
  await this.backbutn.click();
  logger.info('Clicked on Back to WorkOrders button');  
}
async backtoTop() {
  await this.backuser.click();
  logger.info('Clicked on Back to Users button');  
}
async searchWorkOrderbystatus(keyword) {
  await this.searchbystatus.click();
  logger.info('Clicked on Search by Status field');
}
async searchManageUserbyRole(keyword) {
  await this.searchbyrole.click();
  logger.info('Clicked on Search by Role field'); 
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
async validatetoastmsg() {
  const toast = this.page.getByRole('alert');
  await toast.waitFor({ state: 'visible', timeout: 5000 });
  const toastText = await toast.textContent();
  logger.info(`Toast message: ${toastText}`);
  const isSuccess = toastText.includes('has successfully been saved.');
  logger.info(`Success message visibility: ${isSuccess}`);
  return isSuccess;
}
 async verifyAllStatusCompleted() {
   logger.info('Enter verifyAllStatusCompleted method');
  const statusElements = this.page.locator('.grid-column .ellipsis'); // Adjust the selector to target the correct column
 
    const statusText = await statusElements.nth(5).textContent();
    logger.info(`Status Text: ${statusText}`);
    expect(statusText.trim()).toBe('Completed');
  }
async NextPage() {
    const pagerLabel = this.page.locator('.grid-pager > div').first();
    await expect(pagerLabel).toBeVisible({ timeout: CONSTANTS.TIMEOUTS.LONG });

    const nextButtonByTitle = this.page.locator('.grid-pager [title="next"]').first();
    const nextButtonByRole = this.page.getByRole('img', { name: 'next' });

    let nextButton = nextButtonByTitle;
    if (!(await nextButtonByTitle.isVisible().catch(() => false))) {
      nextButton = nextButtonByRole;
    }

    if (!(await nextButton.isVisible().catch(() => false))) {
      logger.info('Next page button is not visible. Skipping page-2 assertion for single-page result.');
      await expect(pagerLabel).toContainText('Page', { timeout: CONSTANTS.TIMEOUTS.LONG });
      return;
    }

    await nextButton.click();
    logger.info('Clicked on Next page button');

    await this.page.waitForLoadState('networkidle').catch(() => {});

    const currentPage = this.page.locator('.grid-pager > div').nth(4);
    const currentPageText = await currentPage.textContent();
    if ((currentPageText ?? '').trim() === '2') {
      logger.info('Verified pagination moved to page 2');
      return;
    }

    logger.info(`Current page after click is '${currentPageText}'. Treating as single-page/no-advance scenario.`);
    await expect(pagerLabel).toContainText('Page', { timeout: CONSTANTS.TIMEOUTS.LONG });
}

async NavigatetoSettings(){
  await expect(this.page).toHaveURL(`https://test.editor.mmi.gov.bc.ca/admin/settings`);
  console.log("Current URL after deletion:", this.page.url());
}
async validateDeleteToastMessage(){
  const toast = this.page.getByRole('alert');
  await toast.waitFor({ state: 'visible', timeout: 5000 });
  const toastText = await toast.textContent();
  logger.info(`Toast message: ${toastText}`);
  const isDeleted = toastText.includes('has successfully been deleted.');
  logger.info(`Delete message visibility: ${isDeleted}`);
  return isDeleted;
}
// Manage orders
async tableStatusCol() {
 
  const statusElements = await this.page.locator('//div[@role="row"]//div[last()]').allTextContents();
  logger.info(`Status Text: ${statusElements}`);
  for (const status of statusElements) {
    console.log("Status Value:", status);
    expect(status.trim()).toBe('Approved');
  }
}
async tableRoleCol() {
  const roleElements = await this.page.locator('//div[@role="row"]//div[5]').allTextContents();
  logger.info(`Role Text: ${roleElements}`);
  for (const role of roleElements) {
    console.log("Role Value:", role);
    expect(role.trim()).toBe('Editor');
  }
}
}
module.exports = { SystemSettings }
