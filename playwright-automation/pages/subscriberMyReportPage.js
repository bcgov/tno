const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');

class SubscriberMyReportPage extends BasePage {
  constructor(page) {
    super(page);

    this.reportNameUnderMyReports = page.locator('.section-label .report-name');
    this.filterReportsInput = page.getByPlaceholder('Filter reports...');
    this.filterReportsButton = page.getByRole('button', { name: 'Filter' });
  }

   /**
   * Method to check if headline is present in the searched result
   * @param {string} reportTitle 
   * @returns {boolean} true if present else false
   */
  async isPublishedReportPresent(reportTitle) {
    logger.info('Verifying published reportTitle in My Reports grid..');
    for (let attempt = 0; attempt < 6; attempt++) {
      if (await this.isElementVisible(this.filterReportsInput, 3000)) {
        await this.filterReportsInput.fill('');
        await this.filterReportsInput.fill(reportTitle);
        await this.filterReportsButton.click();
      }

      await this.page.waitForTimeout(2000);
      if (await this.isTextPresentInCollection(this.reportNameUnderMyReports, reportTitle)) {
        return true;
      }

      logger.info(
        `Report ${reportTitle} was not visible in My Reports yet. Retry ${attempt + 1}/6.`,
      );
      await this.page.reload({ waitUntil: 'domcontentloaded', timeout: CONSTANTS.TIMEOUTS.LONG });
    }

    return false;
  }

  async isPublicReportAvailable(reportTitle) {
    logger.info(`Checking public reports API for report: ${reportTitle}`);
    const reportsUrl = new URL('/reports', this.page.url()).toString();
    const publicReportsUrl = new URL('/api/subscriber/reports/public', this.page.url()).toString();
    const requestPromise = this.page.waitForRequest(
      (request) =>
        request.url().includes('/api/subscriber/reports/my-reports') &&
        !!request.headers().authorization,
      { timeout: CONSTANTS.TIMEOUTS.LONG },
    );

    await this.page.goto(reportsUrl, {
      waitUntil: 'domcontentloaded',
      timeout: CONSTANTS.TIMEOUTS.LONG,
    });

    const request = await requestPromise;
    const authorization = request.headers().authorization;
    const response = await this.page.request.get(publicReportsUrl, {
      headers: { Authorization: authorization },
    });

    if (!response.ok()) {
      throw new Error(`Failed to fetch public reports: ${response.status()}`);
    }

    const reports = await response.json();

    return reports.some((report) => report.name === reportTitle);
  }

}

module.exports = { SubscriberMyReportPage };
