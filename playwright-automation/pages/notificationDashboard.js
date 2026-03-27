const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { getFilePath } = require('../utils/fileUpload.util');
const { expect } = require('@playwright/test');

class NotificationDashboardPage extends BasePage {
  constructor(page) {
    super(page);
        this.notificationDropDown = page.locator('.rs__input-container').first()
        this.notificationStartDate=page.locator('input[name="startDate"]')
        this.notificationEndDate=page.locator('input[name="endDate"]')
        this.showFailedCheckbox =page.getByRole('checkbox', { name: 'Show failed only' })
        this.searchButton=page.getByRole('button', { name: 'search' })


  }
  

  async selectNotification(source) {
    await this.click(this.notificationDropDown);
    const option = this.page.locator('.rs__option').filter({ hasText: source });
    await this.click(option);
    logger.info(`Entered source: ${source}`);
  }
async enterNotificationDates(startDate, endDate) {
  await this.notificationStartDate.fill(startDate);
  await this.notificationEndDate.fill(endDate);
}

async failedNotificationCheckbox(){
      if(await this.showFailedCheckbox.isChecked()) {
      logger.info('Enabled checkbox is already checked');
      return;
    }else {
    await this.click(this.showFailedCheckbox);
    logger.info('Toggled Is Enabled checkbox!!');
    }

}

async clickNotificationSearchButton(){
  await this.click(this.searchButton)
}
}

module.exports = { NotificationDashboardPage };
