const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class EditTopicsPage extends BasePage {
  constructor(page) {
    super(page);

    this.editTopicsPageHeader = page.locator(`p.list-title`);
    this.topicNameTextInput = page.locator(`input#txt-name`);
    this.issuesButton = page.getByRole('button', { name: 'Issues' });
    this.proactiveButton = page.getByRole('button', { name: 'Proactive' });
    this.saveButton = page.getByRole('button', { name: 'Save' });
    this.searchExistingTopicsInput = page.getByPlaceholder('Search by keyword');

    this.deleteTopicIcon = page.locator(`//button[@title="Remove"]`);

  }

  async isEditPoicPageLoaded() {
    logger.info(
      `Is Edit Topic page loaded : ${await this.isElementVisible(this.editTopicsPageHeader)}`,
    );
    return await this.isElementVisible(this.editTopicsPageHeader);
  }

  async createNewTopic(topicName, topicType) {
    await this.type(this.topicNameTextInput, topicName);
    if (topicType === 'Issues') {
      await this.click(this.issuesButton.first());
    } else {
      await this.click(this.proactiveButton.first());
    }
    await this.click(this.saveButton);
    logger.info(`Saved Topic ${topicName} with type ${topicType}`)
  }

  async searchTopic(topicName) {
    await this.type(this.searchExistingTopicsInput, topicName)
    logger.info(`Searched ${topicName}`);
  }

  async isTopicPresentOnGrid(topicName){
    logger.info(`Is Topic present : ${await this.isElementVisible(await this.page.locator(`(//input[@value="${topicName}"])[2]`))}`);
    return await this.isElementVisible(await this.page.locator(`(//input[@value="${topicName}"])[2]`));
  }

  async deleteTopic(){
    await this.click(this.deleteTopicIcon);
    logger.info(`Clicked on Delete!`);
  }
}

module.exports = { EditTopicsPage };
