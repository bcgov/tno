const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const TabManager = require('../utils/tab-Manager');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');
const { expect } = require ('@playwright/test');

class AddDataImportPage extends BasePage {
  constructor(page) {
    super(page);
    this.page = page;
}
}
module.exports = { AddDataImportPage };