const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const TabManager = require('../utils/tab-Manager');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');

class EditorHomePage extends BasePage {
  constructor(page) {
    super(page);

    // ID Locator
    this.profileIcon = page.locator('#profile');

    // Class Locator
    this.productCards = page.locator('.product-card');

    // Text Locator
    this.dashboardTitle = page.getByText('Dashboard');

    // Role Locator
    this.addToCartButton = page.getByRole('button', { name: 'Add to Cart' });

    // Placeholder
    this.searchInput = page.getByPlaceholder('Search products');

    // XPath
    this.firstHeadlineFromHeadlinesGrid = page.locator('//div[contains(@class,"grid-column")][1]');

    // Data-testid
    this.cartCount = page.getByTestId('cart-count');

    // CSS Advanced
    this.priceLabel = page.locator('.price-label >> nth=0');
    this.radioTVContent = page.locator(
      `//div[contains(@class,"create-new")]/*[@data-tooltip-content="Radio/TV"]`,
    );
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
    const targerIndex = 1 + (rowNumber - 1) * 7;
    const headlinesTitleRow = `//div[contains(@class,'grid-column')][${targerIndex}]//div[@class='clickable']`;
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
    let [newPage] = "";
    switch (contentTitle) {
      case 'Radio/TV':
        [newPage] = await Promise.all([
          this.page.context().waitForEvent('page'),
          this.click(this.radioTVContent),
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
}

module.exports = { EditorHomePage };
