const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { getFilePath } = require('../utils/fileUpload.util');
const { expect } = require('@playwright/test');
const { ReportPage } = require('./reportPage');

class HeadlinesDetailsPage extends BasePage {
  constructor(page) {
    super(page);

    this.headlinesTextBox2 = page.getByRole('textbox', { name: 'Headline *' });
    this.headlinesTextBox = page.locator('textarea#txa-headline');
    this.byLineInput = page.locator('input#txt-byline');
    this.publishButton = page.locator('//button[@type="submit"]');
    this.toastNotification = page.getByRole('alert');
    this.browseUpload = page.locator('.upload-box .body');
    this.summaryTextField = page.locator('.ql-editor');
    this.sourceDropDownField = page.locator('#sel-sourceId input[name="sourceId"]');
    this.tagDropdown = page.locator('input[name="select-tags"]');
    this.saveWithoutPublishButton = page.locator(`//button/div[text()='Save without publishing']`);
    this.prepTime = page.locator(`//input[@name="prep"]`);
    this.deleteButton = page.locator(`//button/div[text()='Delete']`);
    this.nextButton = page.locator(`.submit-buttons button[data-tooltip-content="Next"]`);
    this.unPublishButton = page.locator('//button/div[text()="Unpublish"]');
    this.delete = page.locator('//button/div[text()="Delete"]');
    this.deleteConfirmationButton = page.locator('//button/div[text()="Yes, Delete It"]');
    this.printcontent = page.locator('//*[@data-tooltip-content="Print content"]');
    this.topStoriesCheckbox = page.getByRole('checkbox', { name: 'Top Stories', exact: true });
    this.featuredStoriesCheckbox = page.getByRole('checkbox', {
      name: 'Featured Stories',
      exact: true,
    });
    this.commentaryCheckbox = page.locator(`//input[@id="actions.4.placeholder-true"]`);
    this.commentaryCheckboxForPrintContent = page.locator(`//input[@id="actions.6.placeholder-true"]`);
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
    const matchingToast = this.toastNotification.filter({ hasText: headlinesTitle }).first();
    await expect(matchingToast).toContainText(headlinesTitle, {
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });
    await expect(matchingToast).toContainText('has successfully been saved.', {
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });
    return await this.isElementVisible(matchingToast);
  }

  async isToastNotificationVisible(headlinesTitle) {
    try {
      return await this.verifyToastNotificationVisible(headlinesTitle);
    } catch (error) {
      logger.info(`Toast notification was not visible for headline: ${headlinesTitle}`);
      return false;
    }
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
 async inputHeadLineTitle(headlineTitle){
    await this.type(this.headlinesTextBox2, headlineTitle);
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
    logger.info(`Clicked on Print COntent.`);
  }
/** Method to click on Print COntent */
 async clickonPrintcontent() {
    await this.click(this.printcontent);
    await this.hardWait(2000);
    logger.info(`Clicked on Print Content.`);
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

  /**
   * Method to click on UnPublish button.
   */
  async unPublishHeadlines() {
    await this.hardWait(1000);
    await this.click(this.unPublishButton);
    logger.info(`Clicked on UnPublish button`);
  }

  /**
   * Verify visibility of UnPublish button
   */
  async isUnpublishButtonVisible() {
    logger.info(`Un Publish button visibilty status ${await this.isElementVisible(this.unPublishButton)}`);
    return await this.isElementVisible(this.unPublishButton);
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
  async deleteUnpublishedHeadline() {
    logger.info(`Deleting the Unpublished headline`);
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
   * Method to verify Publish button state
   * @returns true if enable else false
   */
  async isPublisheButtonClickable() {
    const isClickable = await this.isElementClickable(this.publishButton);
    logger.info(`Is Publish button clicable - ${isClickable}`);
    return isClickable;
  }

  /**
   * Method to click on top stories or Commentary checkboxes
   * @param {string} filterName
   */
  async selectTopStoriesOrCommentaryChecbox(filterName) {
    switch (filterName) {
      case CONSTANTS.HEADLINES.TOP_STORIES:
        await this.click(this.topStoriesCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.TOP_STORIES}`);
        break;
      case CONSTANTS.HEADLINES.FEATURE_STORIES:
        await this.click(this.featuredStoriesCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.FEATURE_STORIES}`);
        break;
      case CONSTANTS.HEADLINES.COMMENTARY:
        if(await this.isElementVisible(await this.commentaryCheckbox)){
          await this.click(await this.commentaryCheckbox);
        } else {
           await this.click(await this.commentaryCheckboxForPrintContent);
        }
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.COMMENTARY}`);
        break;
      default:
        logger.info(`Invalid Filter Name}`);
        break;
    }
  }
}

module.exports = { HeadlinesDetailsPage };
