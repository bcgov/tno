const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');


class SubscriberNavBarPage extends BasePage {
  constructor(page) {
    super(page);
    this.menuNavigationLink = page.locator('div.nav-item .dropdown-toggle');
    this.mufolderLnk= page.getByText('My Folders');
    this.createFolderText = page.getByRole('textbox', { name: 'Enter name...' })
    this.createNewFolderBtn = page.getByRole('button', { name: 'Create new folder' })
    this.deleteFolderBtn = page.getByRole('button', { name: 'Delete folder' })
    this.confirmDeleteFolderBtn = page.getByText('Yes, Delete Folder')
  }


  /**
   * Method to click on given My Content Section from nav bar
   * @param {string} My Content section name
   */
  async clickOnMyContentSectionByText(navBarOptionName) {
    await this.click(this.page.locator(`//*[@class='group-section']//div[text()='${navBarOptionName}']`));
    logger.info(`Clicked on My Content section option: ${navBarOptionName}`);
    this.hardWait(1000);
  }
  

  async clickOnMyFolder() {
    await this.click(this.mufolderLnk);
    logger.info(`Clicked on My Folder link!!`);
  }

  async enterFolderName(folderName) {
    await this.type(this.createFolderText, folderName);
    logger.info(`Entered folder name as ${folderName}!!`);
  }
  async clickOnCreateNewFolder() {
    await this.click(this.createNewFolderBtn);
    logger.info(`Clicked on Create New Folder button!!`);
  }
  async clickOnDeleteFolder() {
    await this.click(this.deleteFolderBtn);
    await this.click(this.confirmDeleteFolderBtn);
    logger.info(`Clicked on Delete Folder button!!`); 

}
}
module.exports = { SubscriberNavBarPage };
