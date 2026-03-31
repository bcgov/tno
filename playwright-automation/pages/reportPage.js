const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { expect } = require('@playwright/test');

class ReportPage extends BasePage {
  constructor(page) {
    super(page);

    this.addNewReport = page.locator('button[label="Add new report"]');
    this.backToReport = page.locator('button[label="Back to reports"]');
    this.reportName = page.locator('input#txt-name');
    this.reportDescription = page.locator('textarea#txa-description');
    this.applyOwnershipToFilter = page.locator(
      '//button/div[text()="Apply ownership to filters/folders in report"]',
    );
    this.save = page.locator('.form-actions button[type="submit"]');
    this.toastNotification = page.locator('.Toastify .Toastify__toast-body div:nth-child(2)');
    this.useDefaultTemplateButton = page.locator('//button/div[text()="Use Default Template"]');
    this.enableEditCheckbox = page.locator('input#enableEdit-true');
    this.sendTestEmailToField = page.locator('//input[@name="to"]');
    this.sendButton = page.locator('.frm-in button[type="button"]');
    this.delete = page.locator('//button/div[text()="Delete"]');
    this.searchTextBox = page.locator('input[name="search"]');
    this.deleteConfirmationButton = page.locator('//button/div[text()="Yes, Remove It"]');
    this.isPublicCheckbox = page.locator(`#isPublic-true`);
    this.isSortedEnableCheckbox = page.locator(`isEnabled-true`);
    this.nameFieldValidationMessage = page.locator(`.error p`)
    this.sortOrderInputTextField = page.locator(`input#txt-sortOrder`);
    this.validationErrorToastMessage = page.locator(`.Toastify .Toastify__toast-body div:nth-child(2)`);
  }

  /**
   * Method to check editor home page is loaded.
   */
  async verifyReportPageLoaded() {
    await this.addNewReport.waitFor({
      state: 'visible',
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });
    await this.addNewReport.waitFor({
      state: 'attached',
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });
    await this.hardWait(2000);
    logger.info('Report page loaded successfully!!');
  }

  /**
   * Method to click on Add new Report button
   */
  async clickOnAddNewReportButton() {
    await this.click(this.addNewReport);
    logger.info('Clicked on Add new report button');
  }

  /**
   * Method to check visibility of Back to Report button
   * @returns treu if visible else false
   */
  async isBackToReportButtonVisible() {
    logger.info('Checking visibility of Back to Report button');
    return await this.isElementVisible(this.backToReport);
  }

  /**
   * Method to enter Report Name
   * @param {String} Report Name
   */
  async addReportName(reportName) {
    await this.type(this.reportName, reportName);
    logger.info(`Entered Report Name is : ${reportName}`);
  }

  /**
   * Method to enter Report Description
   * @param {String} Report Description
   */
  async addReportDescription(reportDescription) {
    await this.type(this.reportDescription, reportDescription);
    logger.info(`Entered Report Name is : ${reportDescription}`);
  }

  /**
   * Method to click on Apply Ownership to Filter button
   */
  async selectApplyOwnershipToFilters() {
    await this.click(this.applyOwnershipToFilter);
    logger.info('Clicked on Apply Ownership to Filter button');
  }

  /**
   * Method to check Totast Notification's visibility
   * @param {String} reportTitle
   * @returns true is Toast Notification is visible else false
   */
  async verifySucessToastNotification(reportTitle) {
    logger.info(`Verifying visibility of Success Toast Notification Message`);
    await expect(this.toastNotification.first()).toHaveText(
      `${reportTitle} has successfully been saved.`,
    );
    return await this.isElementVisible(this.toastNotification.first());
  }

  /**
   * Method to check the visibility of Reports sub tab
   * @param {string} tabName
   * @returns true if visible else false
   */
  async isReportSubTabVisible(tabName) {
    logger.info(`Verifying visibility of Subtab - ${tabName}`);
    return await this.isElementVisible(
      this.page.locator(`//div[contains(@class,'tab-menu')]//span[text()='${tabName}']`),
    );
  }

  /**
   * Method to click on given reports subtab
   * @param {string} subTab Name
   */
  async clickOnReportSubTab(tabName) {
    await this.click(
      this.page.locator(`//div[contains(@class,'tab-menu')]//span[text()="${tabName}"]`),
    );
    logger.info(`Clicked on SubTab button ${tabName}`);
  }

  /**
   * Method to verify button state
   * @returns true if enable else false
   */
  async isUseDefaultTemplateButtonClickable() {
    const isClickable = await this.isElementClickable(this.useDefaultTemplateButton);
    logger.info(`Is Publish button clicable - ${isClickable}`);
    return isClickable;
  }

  /**
   * Methos to select Enable Edit Template check box on report page
   */
  async checkEnableEditCheckbox() {
    await this.hardWait(2000);
    await this.click(this.enableEditCheckbox);
    logger.info('Clicked on Enable Edit Template checkbox.');
  }

  /**
   * Method to click on Use Default Template button.
   */
  async clickOnUseDefaultTemplateButton() {
    await this.click(this.useDefaultTemplateButton);
    logger.info('Clicked on Use Default Template button.');
  }

  /**
   * Method to click on Save button.
   */
  async clickOnSaveButton() {
    await this.click(this.save.first());
    await this.hardWait(2000);
    logger.info('Clicked on Save button.');
  }

  /**
   * Method to click on Report Section button
   * @param {string} report section Name
   */
  async clickOnReportSectionButtonByName(reportSectionName) {
    await this.click(this.page.locator(`//button/div[text()='${reportSectionName}']`));
    logger.info(`Clicked on SubTab button ${reportSectionName}`);
  }

  /**
   * Method to enter email addres in Send Email to text field
   * @param {String} email Address
   */
  async sendTestEmailPreview(email) {
    await this.type(this.sendTestEmailToField, email);
    logger.info(`Entered email text ${email}`);
    await this.click(this.sendButton);
  }

  /**
   * Method to click on Delete button.
   */
  async clickOnDeleteButton() {
    await this.click(this.delete.first());
    await this.hardWait(2000);
    logger.info('Clicked on Delete button.');
  }

   /**
   * Method to delete the given report from the grid.
   */
  async searchAndDeleteReport(reportName) {
    logger.info(`Searching for Report : ${reportName}`);
    await this.type(this.searchTextBox, reportName);
    await this.click(
      this.page.locator(`//div[text()='${reportName}']`),
    );
    await this.hardWait(1000);
    await this.clickOnDeleteButton();
    await this.click(this.deleteConfirmationButton);
  }

  /**
   * Method to check Delete Totast Notification's visibility
   * @param {String} reportTitle
   */
  async verifyDeleteSucessToastNotification(reportTitle) {
    logger.info(`Verifying visibility of Delete Success Toast Notification Message.`);
    await this.hardWait(2000);
    await expect(this.toastNotification.first()).toHaveText(
      `${reportTitle} has successfully been deleted.`,
    );
  }

  /**
   * Method to check visibility of Add New Report button
   * @returns treu if visible else false
   */
  async isAddNewReportButtonVisible() {
    logger.info('Checking visibility of Add New Report button');
    return await this.isElementVisible(this.addNewReport);
  }

  /**
   * Method to select is Public check box on report page
   */
  async checkIsPublicCheckbox() {
    await this.click(this.isPublicCheckbox);
    logger.info('Clicked on Is Public checkbox.');
  }

  /**
   * Method to select given report from the grid.
   */
  async searchAndSelectReport(reportName) {
    logger.info(`Searching for Report : ${reportName}`);
    await this.type(this.searchTextBox, reportName);
    await this.click(
      this.page.locator(`//div[text()='${reportName}']`),
    );
    await this.hardWait(1000);
  }

  /**
   * Method to select is Sorted enable check box on report page
   */
  async checkIsSortOrderIsEnabledCheckbox() {
    await this.click(this.isSortedEnableCheckbox);
    logger.info('Clicked on Is Sort Order Enabled checkbox.');
  }

  /**
   * Method to check sort order is enabled check box selected
   * @returns true if selected else false
   */
  async isSortOrderEnabledCheckboxSelected() {
    const isSelected = await this.isCheckboxSelected(this.isSortedEnableCheckbox);
    logger.info(`Is Sort Order Enable checkbox selected - ${isSelected}`);
    return isSelected;
  }

  /**
   * Method to get entered Report Description
   */
  async getReportDescription() {
    const reportDescriptionText = await this.getInputValue(this.reportDescription);
    logger.info(`Entered Report description is : ${reportDescriptionText}`);
    return reportDescriptionText;
  }

  /**
   * Method to get the Report Name field validation message
   * @returns Validation message for Report Name field
   */
  async getNameFieldValidationMessageLable() {
    logger.info(`Validation message for Report Name is : ${await this.getElementText(this.nameFieldValidationMessage)}`);
    return await this.getElementText(this.nameFieldValidationMessage);
  }

  /**
   * Method to get th Toast validation message
   * @returns Validation message for Report Name field
   */
  async getToastValidationMessage() {
    logger.info(`Validation message On Notification toast is : ${await this.getElementText(this.validationErrorToastMessage)}`);
    return await this.getElementText(this.validationErrorToastMessage);
  }

  /**
   * Method to enter sort order value
   * @param {string} sortOrder 
   */
  async enterSortOrder(sortOrder){
    await this.type(this.sortOrderInputTextField, sortOrder);
    logger.info(`Enter sort order : ${sortOrder}`);
  }


}

module.exports = { ReportPage };
