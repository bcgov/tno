const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class SubscriberMyReportPage extends BasePage {
  constructor(page) {
    super(page);

    this.reportNameUnderMyReports = page.locator('.section-label .report-name');
  }

   /**
   * Method to check if headline is present in the searched result
   * @param {string} reportTitle 
   * @returns {boolean} true if present else false
   */
  async isPublishedReportPresent(reportTitle) {
    logger.info('Verifying published reportTitle in My Reports grid..');
    return this.isTextPresentInCollection(this.reportNameUnderMyReports, reportTitle);
  }

}

module.exports = { SubscriberMyReportPage };
