const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const TabManager = require('../utils/tab-Manager');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');

class EditorOnlineStoryPage extends BasePage {
  constructor(page) {
    super(page);
this.featuredStoriesCheckbox = page.getByRole('checkbox', { name: 'Featured Stories' })
this.topStoriesCheckbox = page.getByRole('checkbox', { name: 'Top Stories' })

this.commentaryCheckbox = page.getByRole('checkbox', { name: 'Commentary' })
this.headlineInput = page.getByRole('textbox', { name: 'Headline *' })
this.tnoOption = page.locator('#react-select-2-placeholder')
this.tnoOptionSelect = this.page.locator('#react-select-2-option-0')

this.paragraph=page.getByRole('paragraph')
this.paragraphEditor =  page.locator('.ql-editor');

this.publishButton = page.getByRole('button', { name: 'Publish', exact: true });
this.sentimentPoints=page.getByRole('button', { name: '4', exact: true });

this.toastMessage = page.locator('[role="alert"]');
this.featuredClick=page.getByText('Featured Stories', { exact: true });

this.linkOnlineStoryWithRandomNum = page.getByRole('link', { name: 'Test Online Story Headline -' }).first()

this.contentConfigurationButton = page.getByRole('button', { name: 'Content Configuration' })
this.ministersLink = page.getByRole('link', { name: 'Ministers' })
this.addNewMinisterButton = page.getByRole('button', { name: 'plus Add new minister' })
this.ministerNameInput = page.getByRole('textbox', { name: 'Name' })
this.ministerDescriptionInput = page.getByRole('textbox', { name: 'Description' })
this.ministerPositionInput = page.getByRole('textbox', { name: 'Position' })
this.ministerSortOrderInput = page.getByRole('spinbutton', { name: 'Sort Order' })
this.saveMinisterButton = page.getByRole('button', { name: 'Save' })
this.backToMinistersButton = page.getByRole('button', { name: 'back Back to Ministers' })
this.searchByKeywordInput = page.getByRole('textbox', { name: 'Search by keyword' })
this.resetButton = page.getByRole('button', { name: 'reset' })


  }


  async contentCreationOnline(mmiMSUrl) {
  await this.page.goto(mmiMSUrl+'stories/0')
  await this.page.waitForLoadState('networkidle');
  logger.info(`Navigated to Online Story Creation page!!`);
}


async selectFeaturedStories() {
  await this.featuredStoriesCheckbox.check();
  logger.info(`Selected Featured Stories filter!!`);
}

async selectTopStories() {
  await this.topStoriesCheckbox.check();
  logger.info(`Selected Top Stories filter!!`);
} 

async selectCommentary() {
  await this.commentaryCheckbox.check();
  logger.info(`Selected Commentary filter!!`);
}


async enterHeadline(headline) {
  await this.headlineInput.type(headline);
  logger.info(`Entered headline: ${headline}`);
}

async selectTNOOption() {
  await this.tnoOption.click({ force: true });
  await this.tnoOptionSelect.click({force: true});
  logger.info(`Selected TNO option!!`);
}

async enterParagraph(paragraph) {
  await this.paragraph.click();
  await this.paragraphEditor.type(paragraph);
  logger.info(`Entered paragraph: ${paragraph}`);
}


async selectPointsAndClickPublish() {
  await this.sentimentPoints.click();
  await this.publishButton.click();
  logger.info(`Clicked on Publish button!!`);
}

async getToastMessage() {
  const message = await this.toastMessage.textContent();
  logger.info(`Toast message: ${message}`);
  return message;
}

async navigatefeaturedStories() {
  await this.featuredClick.click();
  logger.info(`Clicked on Featured Stories filter!!`);
}




  async verifyOnlineStory(randomNum) {
    const message = await this.page
      .getByRole('link', { name: `Test Online Story Headline - ${randomNum}` })
      .first()
      .textContent();

    return message;
  }





// Scenario 2 methods


async navigateToContentConfiguration() {
  await this.contentConfigurationButton.click();
  logger.info(`Clicked on Content Configuration button!!`);
}


async navigateToMinisters() {
  await this.ministersLink.click();
  logger.info(`Clicked on Ministers link!!`);
}


async clickAddNewMinister() {
  await this.addNewMinisterButton.click();
  logger.info(`Clicked on Add New Minister button!!`);
}

async enterMinisterDetails(name, description, position, sortOrder) {
  await this.ministerNameInput.type(name);
  console.log(`Minister Name entered: ${name}`);
  await this.ministerDescriptionInput.type(description);
  await this.ministerPositionInput.type(position);
  await this.ministerSortOrderInput.type(sortOrder.toString());
  await this.saveMinisterButton.click();
  logger.info(`Entered minister details: Name=${name}, Description=${description}, Position=${position}, Sort Order=${sortOrder}`);
}



async clickBackToMinisters() {
  await this.backToMinistersButton.click({ force: true });
  logger.info(`Clicked on Back to Ministers button!!`);
}


async searchMinisterByName(name) {
  await this.searchByKeywordInput.type(name);
  logger.info(`Searched for minister with name: ${name}`);

}

async getSearchedMinisterName(name) {
  const searchedMinisterName = await this.page.locator('div').filter({ hasText: name }).first()
.textContent();
  logger.info(`Searched Minister Name: ${searchedMinisterName}`);
  return searchedMinisterName;
}









}

module.exports = { EditorOnlineStoryPage };
