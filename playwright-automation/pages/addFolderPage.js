const BasePage = require('./base/BasePage');
const logger = require('../utils/logger');

class AddFolderPage extends BasePage {
  constructor(page) {
    super(page);

    this.backToFoldersButton = page.getByRole('button', { name: 'Back to folders' });
    this.nameInput = page.locator('#txt-name');
    this.descriptionInput = page.locator(`#txa-description`);

    this.searchFoldersInput = page.getByPlaceholder('Search by keyword');
    this.searchFoldersButton = page.locator(`//button//*[@alt="search"]`);

    this.saveButton = page.getByRole('button', { name: 'Save' });

    this.deleteButton = page.getByRole('button', { name: 'Delete' });
    this.deleteConfirmation = page.locator('//button/div[text()="Yes, Remove It"]');

    this.filterInputTextBox = page.locator(`input[id*='react-select']`)
    this.folderLink = page.locator(`.ellipsis`);

    this.toastNotification = page.locator('.Toastify .Toastify__toast-body div:nth-child(2)');

  }

  /**
   * Method to check visibility of back to folders button.
   */
  async verifyBackToFolderssVisibility() {
    logger.info(`Visibility of button is ${await this.isElementVisible(this.backToFoldersButton)}`);
    return await this.isElementVisible(this.backToFoldersButton);
  }

  /**
   * Method to enter Folders detail
   * @param {string} name
   */
  async enterFoldersDetails(name, description) {
    await this.type(this.nameInput, name);

    await this.type(this.descriptionInput, description)

    logger.info(`Added Folders Details.`);
  }

  /**
   * Method to add folder description
   * @param {*} description 
   */
  async addFolderDescription(description) {
     await this.type(this.descriptionInput, description)

    logger.info(`Added Folders Description.`);
  }

  /**
   * Method to select given produc
   * @param {string} folderName 
   */
  async selectFolder(folderName) {
    await this.type(this.searchFoldersInput, folderName);
    await this.page.keyboard.press('Enter');
    await this.hardWait(500);
    logger.info(`Total product shown on grid : ${(await this.folderLink.nth(1)).count()}`);
    await this.click(this.folderLink.first());
    logger.info(`Selected product : ${folderName}`);
  }

  /**
   * Method to save the Folder details.
   */
  async save(){
    await this.click(this.saveButton);
    await this.hardWait(2000);
    logger.info(`Clicked on Save Button`);
  }

  /**
   * Method to click on given folders subtab
   * @param {string} subTab Name
   */
  async clickOnFoldersSubTab(tabName) {
    await this.click(
      this.page.locator(`//div[contains(@class,'tab-menu')]//span[text()="${tabName}"]`),
    );
    logger.info(`Clicked on SubTab : ${tabName}`);
  }


  /**
   * Method to go back to Folders grid
   */
  async backToFoldersGrid() {
    await this.click(this.backToFoldersButton);
    logger.info(`Clicked on Back to Folders button`);
  }

  /**
   * Method to check visibility of Folders on the grid
   * @param {string} productName 
   * @returns true if visible
   */
  async isAddedFolderVisibleOnGrid(foldertName) {
    logger.info(`Is Added folder Visible : ${await this.isTextPresentInCollection(this.folderLink, foldertName)}`);
    return this.isTextPresentInCollection(this.folderLink, foldertName);
  }

  /**
   * Method to select given filter
   * @param {string} filterName 
   */
  async selectFilter(filterName) {
    await this.type(this.filterInputTextBox, filterName);
    await this.page.keyboard.press('Enter');
    await this.hardWait(500);
    logger.info(`Selected product : ${filterName}`);
  }

  /**
   * Method to delete the Folder
   */
  async deleteProduct() {
    await this.click(this.deleteButton);
    await this.click(this.deleteConfirmation);
    logger.info(`Successfully Deleted the newly added folder.`);
  }

  /**
   * Visiility of Success Toast message
   * @returns true if present else false
   */
  async isSuccessToastNotificationDisplayed(){
    logger.info(`Is Success Toast visible : ${ await this.isElementVisible(this.toastNotification)}`);
    return await this.isElementVisible(this.toastNotification);
  }

}

module.exports = { AddFolderPage };
