const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { getFilePath } = require('../utils/fileUpload.util');
const { expect } = require('@playwright/test');

class HeadlinesDetailsPage extends BasePage {
  constructor(page) {
    super(page);

    this.headlinesTextBox = page.locator('textarea#txa-headline');
    this.byLineInput = page.locator('input#txt-byline');
    this.publishButton = page.locator('//button[@type="submit"]');
    this.toastNotification = page.locator('.Toastify .Toastify__toast-body div:nth-child(2)');
    this.browseUpload = page.locator('.upload-box .body');
    this.summaryTextField = page.locator('.ql-editor');
    this.sourceDropDownField = page.locator('#sel-sourceId input[name="sourceId"]');
    this.tagDropdown = page.locator('input#react-select-7-input');
    this.saveWithoutPublishButton = page.locator(`//button/div[text()='Save without publishing']`);
    this.prepTime = page.locator(`//input[@name="prep"]`);
    this.deleteButton = page.locator(`//button/div[text()='Delete']`);
    this.nextButton = page.locator(`.submit-buttons button[data-tooltip-content="Next"]`);
  }

  /**
   * Method to check Headlines details page is loaded.
   */
  async verifyHeadlinesDetailsPageLoaded() {
    await this.hardWait(5000);
    await expect(this.byLineInput).toBeVisible({timeout: CONSTANTS.TIMEOUTS.LONG });
    logger.info('Headlines details page loaded successfully!!');
  }

  /**
   * Methos to get the text from Headlines text field
   * @returns Headlines title
   */
  async getHeadlinesTextFieldValue() {
    await this.hardWait(2000);
    const headlinesTitle = await this.getInputValue(this.headlinesTextBox);
    logger.info(`Headlines title is ${headlinesTitle}`);
    return headlinesTitle;
  }

  /**
   * Method to enter value in byline text field
   * @param {String} bylineText
   */
  async enterByline(bylineText) {
    await this.type(this.byLineInput, bylineText);
    logger.info(`Entered byline text ${bylineText}`);
  }

  /**
   * Method to click on Publish button.
   */
  async publishHeadlines() {
    await this.click(this.publishButton);
    logger.info(`Clicked on Publish button`);
  }

  /**
   * Method to click on Sentiment button
   * @param {String} value 
   */
  async clickOnSentimentButtonByText(value) {
    await this.click(this.page.locator(`//*[@label="Sentiment"]//button[text()="${value}"]`));
    logger.info(`Clicked on Sentiment button ${value}`);
  }

  /**
   * Method to check Totast Notification's visibility
   * @param {String} headlinesTitle 
   * @returns true is Toast Notification is visible else false
   */
  async verifyToastNotificationVisible(headlinesTitle) {
    logger.info(`Verifying visibility of Toast Notification Message`);
    await expect(this.toastNotification.first()).toHaveText(`"${headlinesTitle}" has successfully been saved.`)
    return await this.isElementVisible(this.toastNotification.first());
  }

  /**
   * Method to close the tab.
   */
  async closePage() {
    await this.page.close();
     logger.info(`Closed the new tab`);
  }

  /**
   * Upload file using browser
   */
  async uploadRadioTVContentFile(fileName) {
    const filePath = getFilePath(fileName);
    await this.uploadFile(this.browseUpload, filePath);
    logger.info(`Uploaded ${fileName} file successfully.`);
  }

  /**
   * Enter summary
   */
  async enterSummary(summary) {
    await this.type(this.summaryTextField, summary);
    logger.info(`Added Summary details : ${summary}`);
  }

  /**
   * Enter headline title
   */
  async enterHeadLineTitle(headlineTitle){
    await this.type(this.headlinesTextBox, headlineTitle);
    logger.info(`Added headline title : ${headlineTitle}`);
  }

  /**
   * Select source 
   * @param {string} sourceOption 
   */
  async selectSource(sourceOption) {
    await this.type(this.sourceDropDownField, sourceOption);
    await this.page.keyboard.press('Tab');
    logger.info(`Selected source option : ${sourceOption}`);
  }

  /**
   * Enter tag
   * @param {string} tagName 
   */
  async selectTag(tagName) {
    await this.type(this.tagDropdown, tagName);
    await this.page.keyboard.press('Tab');
    logger.info(`Selected source option : ${tagName}`);
  }

  /**
   * Method to click on Publish button.
   */
  async saveHeadlinesWithoutPublish() {
    await this.click(this.saveWithoutPublishButton);
    await this.hardWait(2000);
    logger.info(`Clicked on Save without Publish button.`);
  }

  /**
   * Enter prep time 
   * @param {string} sourceOption 
   */
  async enterPrepTime(prepTime) {
    await this.type(this.prepTime, prepTime);
    logger.info(`Entered prep time values : ${prepTime}`);
  }

  /**
   * Method to check visibility of Delete button 
   */
  async isDeleteButtonVisible() {
    logger.info(`Visibility of Delete button : ${await this.isElementVisible(this.deleteButton)}`);
    return await this.isElementVisible(this.deleteButton);
  }

  /**
   * Method to check visibility of Next preview  button 
   */
  async isNextPreviewButtonVisible() {
    logger.info(`Visibility of Next preview button : ${await this.isElementVisible(this.nextButton)}`);
    return await this.isElementVisible(this.nextButton);
  }


}

module.exports = { HeadlinesDetailsPage };
