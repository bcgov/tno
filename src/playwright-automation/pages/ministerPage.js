const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class MinisterPage extends BasePage {
  constructor(page) {
    super(page);
    this.reportNameUnderMyReports = page.locator('.section-label .report-name');
    this.settings = page.getByText('Settings');
    this.myMinisterLink = page.getByRole('heading', { name: 'My Minister' }).locator('span');
    this.saveButton = page.getByRole('button', { name: 'Save' }).nth(1);
    this.clickMyMinisterMenu = page.getByText('My Minister').first()
  }

  async clickOnSettings() {
    await this.click(this.settings);
    logger.info(`Clicked on Settings link!!`);
  }

  async clickOnMyMinister() {
    await this.click(this.myMinisterLink);
    logger.info(`Clicked on My Minister link!!`);
  }

  geMinisterCheckboxByName(ministerName) {
    return this.page.getByRole('checkbox', { name: ministerName });
  }

  async clickOnMinisterCheckbox(ministerName) {
    const ministerCheckbox = this.geMinisterCheckboxByName(ministerName);
    const isSelected = await ministerCheckbox.isChecked();
    if (isSelected) {
      logger.info(`${ministerName} checkbox is already selected!!`);
      return;
    }else {
      await this.click(ministerCheckbox, { force: true });
      logger.info(`Clicked on ${ministerName} checkbox!!`);
    }
  }


async unClickOnMinisterCheckbox(ministerName) {
    const ministerCheckbox = this.geMinisterCheckboxByName(ministerName);
    const isSelected = await ministerCheckbox.isChecked();
    if (isSelected) {
              await this.click(ministerCheckbox, { force: true });
      logger.info(`unselected on ${ministerName} checkbox!!`);

    }else {
    logger.info(`${ministerName} checkbox is already unselected!!`);
      return;

    }
  }


  async clickOnMinisterSaveButton() {
    await this.click(this.saveButton);
    logger.info(`Clicked on Save button!!`);
  }

  async clickMyMinisterLink() {
    await this.click(this.clickMyMinisterMenu);
    logger.info(`Clicked on My Minister link!!`);
  }
}

module.exports = { MinisterPage };
