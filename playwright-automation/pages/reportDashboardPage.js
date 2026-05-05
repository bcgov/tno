const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');
const { expect } = require('@playwright/test');

class ReportDashboardPage extends BasePage {
  constructor(page) {
    super(page);

    this.reportDashBoardTitle = page.locator('div h1');
    this.showOnlyCheckBox = page.locator('input#failed-true');
    this.failedReportStatus = page.locator('.report-card .failed');
    this.totalRecordsOnGrid = page.locator('.report-card');
    this.expandNextRunForFailedRecordBtn = page.locator(`//div[@class='buttons']//*[local-name()='svg']`);
    this.chesResponseExpandButton = page.locator(`//div[@class='buttons left']//*[local-name()='svg']`);
    this.errorTextArea = page.locator(`//textarea[@name='response']`);

    
  }

  /**
   * Method to get Report Dashboard TItle
   * @returns Report Dashboard Title
   */
  async getReportDashBoardTitle() {
    logger.info(`Report Dashboard title is : ${await this.getElementText(this.reportDashBoardTitle)}`);
    return await this.getElementText(this.reportDashBoardTitle);
  }

  /**
  * Method to click on SHow Only Failed Check box
  */
  async selectOrDeselectShowFailedOnlyCheckbox(){
    await this.click(this.showOnlyCheckBox);
    logger.info(`Clicked on Show only check box}`);
    await this.hardWait(2000);
  }

  /**
   * Method to check visibility of Failed report status
   * @returns treu if visible else false
   */
  async isFailedStatusVisible() {
    logger.info('Checking visibility ofFailed Status');
    return await this.isElementVisible(this.failedReportStatus);
  }

  /**
   * Method to get the total records on the grid
   * @returns Total records present on the Grid
   */
  async getTotalRecordsOnGrid(){
    logger.info(`Records on grid : ${await this.totalRecordsOnGrid.count()}`);
    return await this.totalRecordsOnGrid.count();
  }

   /**
   * Method to check visibility of expand button
   * @returns treu if visible else false
   */
  async isExpandButtonVisible() {
    logger.info('Checking visibility of expand button');
    return await this.isElementVisible(this.expandNextRunForFailedRecordBtn);
  }

  /**
   * Method to click on Expand Button
   */
  async clickExpandButtonUnderNextRunForFailedRecord(){
    await this.click(this.expandNextRunForFailedRecordBtn);
     logger.info(`Clicked on expand button`);
  }

   /**
   * Method to check visibility of CHES Response expand button
   * @returns treu if visible else false
   */
  async isChesResponseExpandButtonVisible() {
    logger.info('Checking visibility of CHES Response expand button');
    return await this.isElementVisible(this.chesResponseExpandButton);
  }

  /**
   * Method to click on CHES Response Expand Button
   */
  async clickChesResponseExpandButtonUnderNextRunForFailedRecord(){
    await this.click(this.chesResponseExpandButton);
     logger.info(`Clicked on CHES Response expand button`);
  }

  /**
   * Method to check visibility of Error text area
   * @returns treu if visible else false
   */
  async isErrorTextAreaVisible() {
    logger.info('Checking visibility of Error Text Area');
    return await this.isElementVisible(this.errorTextArea);
  }
 


}

module.exports = { ReportDashboardPage };
