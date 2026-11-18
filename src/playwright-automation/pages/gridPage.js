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
    logger.info(`Fetching source column values for requested rows: ${totalRows}`);
    const sourceColumnValues = this.page.locator(
      `.content-list .grid-table:nth-child(2) .grid-column:nth-child(7n + 2) .ellipsis`,
    );
    const availableRows = await sourceColumnValues.count();
    const rowsToRead = Math.min(totalRows, availableRows);
    logger.info(`Source rows available: ${availableRows}, reading: ${rowsToRead}`);

    const source = [];

    for (let i = 0; i < rowsToRead; i++) {
      const rawText = (await sourceColumnValues.nth(i).innerText()).trim();
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
    logger.info(`Fetching Media Type column values for requested rows: ${totalRows}`);
    const mediaTypeColumns = this.page.locator(
      `.content-list .grid-table:nth-child(2) .grid-column:nth-child(7n + 3)`,
    );
    const availableRows = await mediaTypeColumns.count();
    const rowsToRead = Math.min(totalRows, availableRows);
    logger.info(`Media Type rows available: ${availableRows}, reading: ${rowsToRead}`);

    const mType = [];

    for (let i = 0; i < rowsToRead; i++) {
      const rawText = (await mediaTypeColumns.nth(i).innerText()).trim();
      if (rawText) {
        mType.push(rawText);
      }
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

  async getSortingIconState(columnName) {
    const sortingIconTitle = this.page
      .locator(
        `//div[contains(@class,"grid-table")]/div[text()='${columnName}']//following-sibling::*[local-name()='svg']/*[local-name()='title']`,
      )
      .or(
        this.page.locator(
          `//header[contains(@class,"header")]//span[text()='${columnName}']//following-sibling::div//*[local-name()='svg']/*[local-name()='title']`,
        ),
      );

    await expect(sortingIconTitle.first()).toBeAttached();
    const stateText = (await sortingIconTitle.first().textContent()).trim().toLowerCase();
    const state = stateText.includes('ascending')
      ? 'ascending'
      : stateText.includes('descending')
        ? 'descending'
        : stateText;
    logger.info(`Sorting icon state for ${columnName}: ${state}`);
    return state;
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
    const normalizedValues = columnValues.map((value) => value.trim());
    const sortedValues = [...normalizedValues].sort((a, b) => {
      if (a === b) return 0;
      return a < b ? -1 : 1;
    });

    switch(sortingOrder.toLowerCase()) {
        case 'ascending' :
            logger.info(`Is sorted ascending: ${JSON.stringify(normalizedValues) === JSON.stringify(sortedValues)}`);
            return JSON.stringify(normalizedValues) === JSON.stringify(sortedValues);

        case 'descending' :
            const isSortedDescending = JSON.stringify(normalizedValues) === JSON.stringify(sortedValues.reverse());
            logger.info(`Is sorted descending : ${isSortedDescending}`);
            return isSortedDescending;

        default: 
            throw new Error(`Invalid sorting order : ${sortingOrder}`);
    }

  }

  async getGridColumnSortDirection(columnValues) {
    const normalizedValues = columnValues.map((value) => value.trim().toLowerCase());
    const sortedValues = [...normalizedValues].sort((a, b) => a.localeCompare(b));
    const isAscending = JSON.stringify(normalizedValues) === JSON.stringify(sortedValues);
    const isDescending = JSON.stringify(normalizedValues) === JSON.stringify([...sortedValues].reverse());

    if (isAscending && isDescending) {
      return 'single-value';
    }

    if (isAscending) {
      return 'ascending';
    }

    if (isDescending) {
      return 'descending';
    }

    return 'unsorted';
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

    const useStatusIcons = this.page.locator(
      `.content-list .grid-table:nth-child(2) .grid-column:nth-child(7n + 7) svg`,
    );
    const totalRows = await useStatusIcons.count();
    if (rowNumber > totalRows) {
      throw new Error(`Row number ${rowNumber} is out of range. Available rows: ${totalRows}`);
    }
    const headlinesRowIconLocator = useStatusIcons.nth(rowNumber - 1);

    await this.click(headlinesRowIconLocator);
    logger.info(`Clicked Use Icon for row : ${rowNumber}`);
  }

  /**
   * Select Use icon for the row with the given headline title.
   * @param {string} headlineTitle
   */
  async selectUseIconByTitle(headlineTitle) {
    const useIcon = this.page.locator(
      `//div[contains(@class,'content-list')]//div[contains(@class,'grid-table')][2]//div[contains(@class,'grid-column')][.//div[contains(@class,'ellipsis') and normalize-space(.)="${headlineTitle}"]]/following-sibling::div[contains(@class,'grid-column')][6]//*[local-name()='svg']`,
    );
    await this.click(useIcon.first());
    logger.info(`Clicked Use Icon for headline : ${headlineTitle}`);
  }

  /**
   * Get Headline title by row number
   * @param { number } rowNumber - Row number starting from 1
   */
  async getTitleByRowNumberOnContentEditorGrid(rowNumber) {
    if (rowNumber < 1) {
      throw new Error('Row number must be greater than 0');
    }

    const titleColumnValues = this.page.locator(
      `.content-list .grid-table:nth-child(2) .grid-column:nth-child(7n + 1) .ellipsis`,
    );
    const totalRows = await titleColumnValues.count();
    if (rowNumber > totalRows) {
      throw new Error(`Row number ${rowNumber} is out of range. Available rows: ${totalRows}`);
    }

    const headlinesRowTitleLocator = titleColumnValues.nth(rowNumber - 1);
    const headlineTitle = (await this.getElementText(headlinesRowTitleLocator)).trim();
    logger.info(`Headline Title is : ${headlineTitle}`);
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
