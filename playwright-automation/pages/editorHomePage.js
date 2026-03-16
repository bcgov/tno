const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');
const { PreviewPage } = require('./previewPage');
const { expect } = require('@playwright/test');

class EditorHomePage extends BasePage {
  constructor(page) {
    super(page);

    // ID Locator
    this.topStoryCheckBox = page.locator('#chk-top-story');
    this.commentaryCheckbox = page.locator('#chk-commentary');
    this.featuredStoriesCheckbox = page.locator('#chk-featuredStories');
    this.publishedCheckbox = page.locator('#chk-published');
    this.readyForReviewCheckbox = page.locator('#ready-true');
    this.inProgressCheckbox = page.locator('#inProgress-true');
    this.failedCheckbox = page.locator('#failed-true');
    this.newsRadioCheckbox = page.locator('#newsRadio-true');

    // Class Locator
    this.productCards = page.locator('.product-card');

    // Text Locator
    this.dashboardTitle = page.getByText('Dashboard');

    // Role Locator
    this.addNewProductButton = page.getByRole('button', { name: 'Add new product' });

    // Placeholder
    this.searchInput = page.getByPlaceholder('Search products');

    // XPath
    this.firstHeadlineFromHeadlinesGrid = page.locator('//div[contains(@class,"grid-column")][1]');
    this.topStoriesCount = page.locator('(//*[@class="paper-totals"]//div)[3]');
    this.commentaryCount = page.locator('(//*[@class="paper-totals"]//div)[6]');
    this.featuredStoriesCount = page.locator('(//*[@class="paper-totals"]//div)[9]');
    this.publishedCount = page.locator('(//*[@class="paper-totals"]//div)[12]');
    this.morningReport = page.locator(`(//*[contains(@class,"button-preview")])[2]`);
    this.sendTopStories = page.locator(`(//*[contains(@class,"button-preview")])[4]`);
    this.sendConfirmationButton = page.locator('//button/div[text()="Yes, Send"]');
    this.cancelSendConfirmation = page.locator(`//button/div[text()="Cancel"]`);

    this.readyForReviewStatusInTranscriptQueueGrid = page.locator(
      `//*[name()="svg"]//*[name()="title" and contains(.,"Ready to review")]`,
    );
    this.inProgressStatsInTranscriptQueueGrid = page.locator(
      `//*[name()="svg"]//*[name()="title" and contains(.,"Submitted")]`,
    );
    this.failedStatusInTranscriptQueueGrid = page.locator(
      `//*[name()="svg"]//*[name()="title" and contains(.,"Failed")]`,
    );
    this.newsRadioStatusInTranscriptQueueGrid = page.locator(``);

    this.totalHeadlinesInGrid = page.locator(`.grid-pager div:nth-child(2)`);

    // Data-testid
    this.cartCount = page.getByTestId('cart-count');

    // CSS Advanced
    this.radioTVContent = page.locator(
      `//div[contains(@class,"create-new")]/*[@data-tooltip-content="Radio/TV"]`,
    );
    this.imageContent = page.locator(
      `//div[contains(@class,"create-new")]/*[@data-tooltip-content="Image"]`,
    );
    this.mediaType = page.locator(`input[name="select-mediaTypeIds"]`);
    this.dailyMediaTypeOption = page.locator(`input[id="Daily Print"]`);
    this.headlineTitleUnderHeadlineColumn = page.locator(`.content-list .ellipsis`);
    this.pageRecordsUpdateTextBox = page.locator(`input[name="quantity"]`);
  }

  /**
   * Method to check editor home page is loaded.
   */
  async verifyEditorHomePageLoaded() {
    await this.firstHeadlineFromHeadlinesGrid.waitFor({
      state: 'visible',
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });
    logger.info('Editor Home page loaded successfully!!');
  }

  /**
   * Click on row title by row number
   * @param { number } rowNumber - Row number starting from 1
   */
  async clickOnHeadlinesTitleByRowNumber(rowNumber) {
    if (rowNumber < 1) {
      throw new Error('Row number must be greater than 0');
    }

    logger.info(`Clicking on headline title for row ${rowNumber}`);
    const targetIndex = 1 + (rowNumber - 1) * 7;
    const headlinesTitleRow = `//div[contains(@class,'grid-column')][${targetIndex}]//div[@class='clickable']`;
    const headlinesRowTitleLocator = this.page.locator(headlinesTitleRow);

    await headlinesRowTitleLocator.waitFor({ state: 'visible', timeout: CONSTANTS.TIMEOUTS.LONG });
    const [newPage] = await Promise.all([
      this.page.context().waitForEvent('page'),
      this.page.click(headlinesTitleRow),
    ]);

    await newPage.waitForLoadState('domcontentloaded');
    logger.info(`Clicked on headline title for row ${rowNumber} and navigated to new tab`);
    await this.hardWait(2000);
    return new HeadlinesDetailsPage(newPage);
  }

  /**
   * Method to click on given content
   */
  async clickOnContent(contentTitle) {
    logger.info(`Clicking on Content title : ${contentTitle}`);
    let [newPage] = '';
    switch (contentTitle) {
      case 'Radio/TV':
        [newPage] = await Promise.all([
          this.page.context().waitForEvent('page'),
          this.click(this.radioTVContent),
        ]);
        break;
      case 'Image':
        [newPage] = await Promise.all([
          this.page.context().waitForEvent('page'),
          this.click(this.imageContent),
        ]);
        break;

      default:
        break;
    }
    await newPage.waitForLoadState('domcontentloaded');
    logger.info(`Clicked on Content title ${contentTitle} and navigated to new tab`);
    await this.hardWait(2000);
    return new HeadlinesDetailsPage(newPage);
  }

  /**
   * Select Media Type filter option daily
   * @param {string} type
   */
  async selectMediaTypeFilterDailyPrint(mType) {
    await this.type(this.mediaType, mType);
    await this.click(this.dailyMediaTypeOption);
    logger.info(`Selected Media type Filter : ${mType}`);
    await this.hardWait(3000);
  }

  /**
   * Method to check if headline is present in the grid
   * @param {string} headline title
   * @returns {boolean} true if present else false
   */
  async isHeadLineTitletPresentOnEditorGrid(headlineTitle) {
    logger.info(`Verifying published headlineTitle in Headline grid : ${headlineTitle}`);
    await this.refreshPage();
    await this.hardWait(4000);
    logger.info(
      `Headline status on grid  : ${await this.isTextPresentInCollection(this.headlineTitleUnderHeadlineColumn, headlineTitle)}`,
    );
    return await this.isTextPresentInCollection(
      this.headlineTitleUnderHeadlineColumn,
      headlineTitle,
    );
  }

  /**
   * Status of button
   * @param {string} buttonName
   * @returns true if enabled else false
   */
  async isButtonEnabledOnPapersEditorGrid(buttonName) {
    const button = this.page.locator(`button:has(div:text-is("${buttonName}"))`);
    logger.info(`Is button enable : ${await this.isElementEnabled(button)}`);
    return await this.isElementEnabled(button);
  }

  /**
   * Method to click on given button name for Papers content on Editor home page
   * @param {string} buttonName
   */
  async clickButtonForPapersContentOnEditorGrid(buttonName) {
    const button = this.page.locator(`button:has(div:text-is("${buttonName}"))`);
    await this.click(button);
    await this.hardWait(2000);
    logger.info(`Clicked on button  : ${buttonName}`);
  }

  /**
   * Method to get the Headline type count
   * @param {string} headlineType
   * @returns count
   */
  async papersTotalCountForGivenHeadlineType(headlineType) {
    let value = '';
    switch (headlineType) {
      case CONSTANTS.HEADLINES.TOP_STORIES:
        value = await this.getElementText(this.topStoriesCount);
        logger.info(`${CONSTANTS.HEADLINES.TOP_STORIES} count is ${value}`);
        break;
      case CONSTANTS.HEADLINES.COMMENTARY:
        value = await this.getElementText(this.commentaryCount);
        logger.info(`${CONSTANTS.HEADLINES.COMMENTARY} count is ${value}`);
        break;
      case CONSTANTS.HEADLINES.FEATURE_STORIES:
        value = await this.getElementText(this.featuredStoriesCount);
        logger.info(`${CONSTANTS.HEADLINES.FEATURE_STORIES} count is ${value}`);
        break;
      case CONSTANTS.HEADLINES.PUBLISHED:
        value = await this.getElementText(this.publishedCount);
        logger.info(`${CONSTANTS.HEADLINES.PUBLISHED} count is ${value}`);
        break;

      default:
        logger.info(`Invalid headline Type : ${headlineType}`);
        break;
    }

    return value;
  }

  /**
   * Method to click on given Advance filer for Papers content
   * @param {string} filterName
   */
  async selectAdvanceSearchTextBoxForPapersContent(filterName) {
    switch (filterName) {
      case CONSTANTS.HEADLINES.TOP_STORY:
        await this.click(this.topStoryCheckBox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.TOP_STORY}`);
        break;
      case CONSTANTS.HEADLINES.COMMENTARY:
        await this.click(this.commentaryCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.COMMENTARY}`);
        break;
      case CONSTANTS.HEADLINES.FEATURE_STORIES:
        await this.click(this.featuredStoriesCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.FEATURE_STORIES}`);
        break;
      case CONSTANTS.HEADLINES.PUBLISHED:
        await this.click(this.publishedCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.PUBLISHED}`);
        break;
      default:
        logger.info(`Invalid Filter Name}`);
        break;
    }

    await this.hardWait(2000);
  }

  /**
   * Method to check visibility of given Advance filer for Papers content
   * @param {string} filterName
   */
  async isAdvanceSearchTextBoxForPapersContentPresent(filterName) {
    let isAdvanceFilterVisible = false;
    switch (filterName) {
      case CONSTANTS.HEADLINES.TOP_STORY:
        isAdvanceFilterVisible = await this.isElementVisible(this.topStoryCheckBox);
        logger.info(`Advance filter option ${CONSTANTS.HEADLINES.TOP_STORY} is visible`);
        break;
      case CONSTANTS.HEADLINES.COMMENTARY:
        isAdvanceFilterVisible = await this.isElementVisible(this.commentaryCheckbox);
        logger.info(`Advance filter option ${CONSTANTS.HEADLINES.COMMENTARY} is visible`);
        break;
      case CONSTANTS.HEADLINES.FEATURE_STORIES:
        isAdvanceFilterVisible = await this.isElementVisible(this.featuredStoriesCheckbox);
        logger.info(`Advance filter option ${CONSTANTS.HEADLINES.FEATURE_STORIES} is visible`);
        break;
      case CONSTANTS.HEADLINES.PUBLISHED:
        isAdvanceFilterVisible = await this.isElementVisible(this.publishedCheckbox);
        logger.info(`Advance filter option ${CONSTANTS.HEADLINES.PUBLISHED} is visible`);
        break;

      default:
        logger.info(`Invalid Filter Name}`);
        break;
    }

    return isAdvanceFilterVisible;
  }

  /**
   * Method to click on preview morning report
   */
  async previewMorningReport() {
    const [newPage] = await Promise.all([
      this.page.context().waitForEvent('page'),
      await this.click(this.morningReport),
    ]);

    await newPage.waitForLoadState('domcontentloaded');
    logger.info(`Clicked on Morning Report preview`);
    await this.hardWait(2000);
    return new PreviewPage(newPage);
  }

  /**
   * Method to send Top storeis
   * @param {string} action - send or cancel
   */
  async clickToSendTopStories(action) {
    await this.click(this.sendTopStories);
    if (action === 'Send') {
      await this.click(this.sendConfirmationButton);
    } else {
      await this.click(this.cancelSendConfirmation);
    }
  }

  /**
   * Select checkbox for the given row number
   * @param { number } rowNumber - Row number starting from 1
   */
  async selectOnHeadlinesCheckBoxByRowNumber(rowNumber) {
    if (rowNumber < 1) {
      throw new Error('Row number must be greater than 0');
    }

    const targetIndex = 1 + (rowNumber - 1) * 7;
    const headlinesRow = `//div[contains(@class,'grid-column')][${targetIndex}]//div[contains(@class,'chk')]`;
    const headlinesRowCheckBoxLocator = this.page.locator(headlinesRow);

    await this.click(headlinesRowCheckBoxLocator);
    logger.info(`Selected Headline checkbox for row : ${rowNumber}`);
  }

  /**
   * Get Headline title by row number
   * @param { number } rowNumber - Row number starting from 1
   */
  async getHeadlinesTitleByRowNumberOnPapersEditorGrid(rowNumber) {
    if (rowNumber < 1) {
      throw new Error('Row number must be greater than 0');
    }

    let headlineTitle = '';
    const targetIndex = 2 + (rowNumber - 1) * 7;
    const headlinesTitleRow = `//div[contains(@class,'grid-column')][${targetIndex}]//div[@class='clickable']`;
    const headlinesRowTitleLocator = this.page.locator(headlinesTitleRow);
    logger.info(`Headline Title is : ${await this.getElementText(headlinesRowTitleLocator)}`);
    headlineTitle = await this.getElementText(headlinesRowTitleLocator);

    return headlineTitle;
  }

  /**
   * Click on row title by row number
   * @param { string } headline title
   */
  async clickOnHeadlinesByGivenTitle(headlineTitle) {
    const headlinesTitleRow = `//*[@class='clickable']/div[text()='${headlineTitle}']`;
    const headlinesRowTitleLocator = this.page.locator(headlinesTitleRow);
    const [newPage] = await Promise.all([
      this.page.context().waitForEvent('page'),
      this.click(headlinesRowTitleLocator),
    ]);

    await newPage.waitForLoadState('domcontentloaded');
    logger.info(`Clicked on headline title ${headlineTitle} and navigated to new tab`);
    await this.hardWait(2000);
    return new HeadlinesDetailsPage(newPage);
  }

  /**
   * Method to click on given Status filter for Transcript queue content
   * @param {string} filterName
   */
  async selectAdvanceSearchTextBoxForTranscriptQueueContent(filterName) {
    switch (filterName) {
      case CONSTANTS.HEADLINES.READY_FOR_REVIEW:
        await this.click(this.readyForReviewCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.READY_FOR_REVIEW}`);
        break;
      case CONSTANTS.HEADLINES.IN_PROGRESS:
        await this.click(this.inProgressCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.IN_PROGRESS}`);
        break;
      case CONSTANTS.HEADLINES.FAILED:
        await this.click(this.failedCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.FAILED}`);
        break;
      case CONSTANTS.HEADLINES.NEWS_RADIO:
        await this.click(this.newsRadioCheckbox);
        logger.info(`Successfully clicked on ${CONSTANTS.HEADLINES.NEWS_RADIO}`);
        break;
      default:
        logger.info(`Invalid Filter Name}`);
        break;
    }

    await this.hardWait(2000);
  }

  /**
   * Method to check status should be same as applied status filter for Transcript queue grid
   * @param {string} statuFilterName
   */
  async verifyAllRecordsHaveSelectedStatusOnTranscriptQueueGrid(statuFilterName) {
    const totalRecordText = await this.getElementText(this.totalHeadlinesInGrid);
    const totalHeadlinesCount = parseInt(totalRecordText.replace(/[()]/g, '').trim());

    logger.info(`Total Records on grid: ${totalHeadlinesCount}`);

    await this.type(this.pageRecordsUpdateTextBox, totalHeadlinesCount.toString());
    await this.page.keyboard.press('Enter');
    await this.page.waitForLoadState('networkidle');
    await this.hardWait(1500);

    let statusCount = '';
    switch (statuFilterName) {
      case CONSTANTS.HEADLINES.READY_FOR_REVIEW:
        statusCount = await this.readyForReviewStatusInTranscriptQueueGrid.count();
        break;

      case CONSTANTS.HEADLINES.IN_PROGRESS:
        statusCount = await this.inProgressStatsInTranscriptQueueGrid.count();
        break;

      case CONSTANTS.HEADLINES.FAILED:
        statusCount = await this.failedStatusInTranscriptQueueGrid.count();
        break;

      default:
        logger.info(`Invalid status filter option.`);
        break;
    }

    logger.info(`Total Records for status ${statuFilterName} on grid is: ${statusCount}`);

    if(statuFilterName === CONSTANTS.HEADLINES.IN_PROGRESS){
      expect(statusCount).toBeLessThanOrEqual(totalHeadlinesCount);
    }
    else if (statusCount !== totalHeadlinesCount) {
      throw new Error(
        `Status Validation failed. Expected ${totalHeadlinesCount} records with status ${statuFilterName} but found ${statusCount}.`,
      );
    }
  }

  /**
   * Method to click on add new method.
   */
  async addNewProduct() {
    await this.click(this.addNewProductButton);
    logger.info(`Clicked on Add new Product button`);
  }
}

module.exports = { EditorHomePage };
