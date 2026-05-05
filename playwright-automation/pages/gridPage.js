const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');
const { PreviewPage } = require('./previewPage');
const { expect } = require('@playwright/test');

class GridPage extends BasePage {
  constructor(page) {
    super(page);

    // ID Locator
    this.showOnlyPublishedCheckbox = page.locator(`#onlyPublished-true`);

    // Class Locator
    this.productCards = page.locator('.product-card');
    this.mmiProductGridRowCount = page.locator(`.grid-table .grid-column .link`);

    // Text Locator
    this.dashboardTitle = page.getByText('Dashboard');

    // Role Locator
    this.addNewProductButton = page.getByRole('button', { name: 'Add new product' });
   

    // Placeholder
    this.searchInput = page.getByPlaceholder('Search products');

    // XPath
    this.firstHeadlineFromHeadlinesGrid = page.locator('//div[contains(@class,"grid-column")][1]');
  }

  /**
   * Method to get the Source from the grid
   * @param {string} totalRows
   * @returns Published dates
   */
  async getSourceColumnDataFromGrid(totalRows) {
    logger.info(`Fetching source column values for total rows: ${totalRows}`);

    const source = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 2 + i * 7;

      const cLocator = `(//div[@class='clickable'])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      source.push(rawText.trim());
    }

    return source;
  }

  /**
   * Method to get the Media Type from the grid
   * @param {string} totalRows
   * @returns Published dates
   */
  async getMediaTypeColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Media Type column values for total rows: ${totalRows}`);

    const mType = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 3 + i * 7;

      const cLocator = `(//div[@class='clickable'])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      mType.push(rawText.trim());
    }

    return mType;
  }


   /**
   * Method to perfrom sorting
   * @param {string} column name
   */
  async performSorting(columnName) {
    const sortingIcon = this.page.locator(`//div[@class="grid-table"]/div[text()='${columnName}']//following-sibling::*[local-name()='svg']`);
    logger.info(`Clicking on sorting Icon for column ${columnName}`);
    await this.click(sortingIcon);
    logger.info(`Clicked on Sort icon`);
    await this.hardWait(1500);
  }

  /**
   * Method to perfrom sorting
   * @param {string} column name
   */
  async performSortingOnFoldersGrid(columnName) {
    const sortingIcon = this.page.locator(`//header[@class="header"]//span[text()='${columnName}']//following-sibling::div//*[local-name()='svg']`);
    logger.info(`Clicking on sorting Icon for column ${columnName}`);
    await this.click(sortingIcon);
    logger.info(`Clicked on Sort icon`);
    await this.hardWait(1500);
  }

  /**
   * Method to check if records are sorted ascending or descending
   * @param {string array} columnValues 
   * @param {string} sortingOrder 
   * @returns true if sorted in given order else false
   */
  async isGridColumnSorted(columnValues, sortingOrder){
    const sortedValues = [...columnValues].sort();

    switch(sortingOrder.toLowerCase()) {
        case 'ascending' :
            logger.info(`Is sorted ascending: ${JSON.stringify(columnValues) === JSON.stringify(sortedValues)}`);
            return JSON.stringify(columnValues) === JSON.stringify(sortedValues);

        case 'descending' :
            const isSortedDescending = JSON.stringify(columnValues) === JSON.stringify(sortedValues.reverse());
            logger.info(`Is sorted descending : ${isSortedDescending}`);
            return isSortedDescending;

        default: 
            throw new Error(`Invalid sorting order : ${sortingOrder}`);
    }

  }

  /**
   * Method to check if Date records are sorted ascending or descending
   * @param {string array} columnValues 
   * @param {string} sortingOrder 
   * @returns true if sorted in given order else false
   */
  async isDateColumnSorted(columnValues, sortingOrder) {
    const values = columnValues.map(text => {
        const [datePart, timePart] = text.trim().split(' ');
        const [month, day, year] = datePart.split('/');
        const formattedDate = `${year}-${month}-${day}T${timePart}`;
        return new Date(formattedDate);
    });

    const sortedValues = [...values].sort((a,b) => a-b);

    switch(sortingOrder.toLowerCase()) {
        case 'ascending' :
            logger.info(`Is sorted ascending: ${JSON.stringify(columnValues) === JSON.stringify(sortedValues)}`);
            return JSON.stringify(values) === JSON.stringify(sortedValues);

        case 'descending' :
            const isSortedDescending = JSON.stringify(values) === JSON.stringify(sortedValues.reverse());
            logger.info(`Is sorted descending : ${isSortedDescending}`);
            return isSortedDescending;

        default: 
            throw new Error(`Invalid sorting order : ${sortingOrder}`);
    }

  }

  /**
   * Select checkbox for the given row number
   * @param { number } rowNumber - Row number starting from 1
   */
  async selectUseIconByRowNumber(rowNumber) {
    if (rowNumber < 1) {
      throw new Error('Row number must be greater than 0');
    }

    const targetIndex = 7 + (rowNumber - 1) * 7;
    const headlinesRow = `//div[contains(@class,'grid-column')][${targetIndex}]//*[local-name()='svg']`;
    const headlinesRowIconLocator = this.page.locator(headlinesRow);

    await this.click(headlinesRowIconLocator);
    logger.info(`Clicked Use Icon for row : ${rowNumber}`);
  }

  /**
   * Get Headline title by row number
   * @param { number } rowNumber - Row number starting from 1
   */
  async getTitleByRowNumberOnContentEditorGrid(rowNumber) {
    if (rowNumber < 1) {
      throw new Error('Row number must be greater than 0');
    }

    let headlineTitle = '';
    const targetIndex = 1 + (rowNumber - 1) * 7;
    const headlinesTitleRow = `//div[contains(@class,'grid-column')][${targetIndex}]//div[@class='clickable']`;
    const headlinesRowTitleLocator = this.page.locator(headlinesTitleRow);
    logger.info(`Headline Title is : ${await this.getElementText(headlinesRowTitleLocator)}`);
    headlineTitle = await this.getElementText(headlinesRowTitleLocator);

    return headlineTitle;
  }

  /**
   * Method to get the Name values from  the MMI Product grid
   * @param {string} totalRows
   * @returns Name
   */
  async getMMIProductNameColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Name column values for total rows: ${totalRows}`);

    const names = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 1 + i * 7;

      const cLocator = `(//div[@class='grid-column '])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      names.push(rawText.trim());
    }

    return names;
  }

  /**
   * Method to get the Has request from  the MMI Product grid
   * @param {string} totalRows
   * @returns Has Request
   */
  async getMMIProductHasRequestColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Has Request column values for total rows: ${totalRows}`);

    const hasRequest = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 2 + i * 7;

      const cLocator = `(//div[@class='grid-column '])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      hasRequest.push(rawText.trim());
    }

    return hasRequest;
  }

  /**
   * Method to get the Description from  the MMI Product grid
   * @param {string} totalRows
   * @returns Has Request
   */
  async getMMIProductDescriptionColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Description column values for total rows: ${totalRows}`);

    const descriptions = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 3 + i * 7;

      const cLocator = `(//div[@class='grid-column '])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      descriptions.push(rawText.trim());
    }

    return descriptions;
  }

  /**
   * Method to get the Type from  the MMI Product grid
   * @param {string} totalRows
   * @returns Description
   */
  async getMMIProductDescriptionColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Description column values for total rows: ${totalRows}`);

    const type = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 4 + i * 7;

      const cLocator = `(//div[@class='grid-column '])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      type.push(rawText.trim());
    }

    return type;
  }

   /**
   * Method to get the Name values from  the Report grid
   * @param {string} totalRows
   * @returns Name
   */
  async getReportGridNameColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Name column values for total rows: ${totalRows}`);

    const names = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 1 + i * 3;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      names.push(rawText.trim());
    }

    return names;
  }

  /**
   * Method to get the Owner from  the Report page grid
   * @param {string} totalRows
   * @returns Owner
   */
  async getReportGridOwnerColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Owner column values for total rows: ${totalRows}`);

    const owner = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 2 + i * 3;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      owner.push(rawText.trim());
    }

    return owner;
  }

  /**
   * Method to get the Description from  the Report page grid
   * @param {string} totalRows
   * @returns Has Request
   */
  async getReportGridDescriptionColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Description column values for total rows: ${totalRows}`);

    const descriptions = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 2 + i * 3;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      descriptions.push(rawText.trim());
    }

    return descriptions;
  }

  /**
   * Method to get the Name values from  the Report grid
   * @param {string} totalRows
   * @returns Name
   */
  async getFiltersGridNameColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Name column values for total rows: ${totalRows}`);

    const names = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 1 + i * 4;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      names.push(rawText.trim());
    }

    return names;
  }

  /**
   * Method to get the Owner from  the Report page grid
   * @param {string} totalRows
   * @returns Owner
   */
  async getFiltersGridOwnerColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Owner column values for total rows: ${totalRows}`);

    const owner = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 3 + i * 4;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      owner.push(rawText.trim());
    }

    return owner;
  }

  /**
   * Method to get the Description from  the Report page grid
   * @param {string} totalRows
   * @returns 
   */
  async getFiltersGridDescriptionColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Description column values for total rows: ${totalRows}`);

    const descriptions = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 2 + i * 4;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      descriptions.push(rawText.trim());
    }

    return descriptions;
  }

  /**
   * Method to get the Name values from  the Folders grid
   * @param {string} totalRows
   * @returns Name
   */
  async getFolderGridNameColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Name column values for total rows: ${totalRows}`);

    const names = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 1 + i * 3;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      names.push(rawText.trim());
    }

    return names;
  }

  /**
   * Method to get the Owner from  the Folder page grid
   * @param {string} totalRows
   * @returns Owner
   */
  async getFolderGridOwnerColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Owner column values for total rows: ${totalRows}`);

    const owner = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 3 + i * 3;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      owner.push(rawText.trim());
    }

    return owner;
  }

  /**
   * Method to get the Description from  the Report page grid
   * @param {string} totalRows
   * @returns Has Request
   */
  async getFolderGridDescriptionColumnDataFromGrid(totalRows) {
    logger.info(`Fetching Description column values for total rows: ${totalRows}`);

    const descriptions = [];

    for (let i = 0; i < totalRows; i++) {
      const index = 2 + i * 3;

      const cLocator = `(//div[contains(@class,'ellipsis')])[${index}]`;
      const rawText = await (await this.page.locator(cLocator).innerText()).trim();
      descriptions.push(rawText.trim());
    }

    return descriptions;
  }

  async getTotalRowCountForMMIProductGgrid(){
    logger.info(`Total rows on MMI product grid : ${await this.mmiProductGridRowCount.count()}`);
    return await this.mmiProductGridRowCount.count();
  }
}

module.exports = { GridPage };
