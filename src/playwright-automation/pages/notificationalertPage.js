const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const TabManager = require('../utils/tab-Manager');
const { HeadlinesDetailsPage } = require('./headlinesDetailsPage');

class NotificationalertPage extends BasePage {
  constructor(page) {
    super(page);

  this.navigatetoNotifBuilder = page.getByRole('button', { name: 'Notifications Building' });
  this.navigatetoNotification = page.getByRole('link', { name: 'Notifications' });
  this.addNotification = page.getByRole('button', { name: 'plus Add new Notification' });
  this.clickonNameandEnter = page.getByRole('textbox', { name: 'Name' })
  this.enterdescription = page.getByRole('textbox', { name: 'Description' })
  this.sortorder = page.getByRole('spinbutton', { name: 'Sort Order' })
  this.saveNotificationbtn = page.getByRole('button', { name: 'Save' })



  }

   async navigateNotifBuild() {
  await this.navigatetoNotifBuilder.click();
  logger.info(`Clicked on Notification button!!`);
}
async notificationMenue() {
  await this.navigatetoNotification.click();
  logger.info(`Clicked on Notification in menue`);
}
async addnotification() {
  await this.addNotification.click();
  logger.info(`Clicked on Add Notification`);
}

async enterDetails(name, description, sortOrder) {
  
  await this.clickonNameandEnter.fill(name);
  console.log(`Notification Name entered: ${name}`);
  
  await this.enterdescription.fill(description);
  await this.sortorder.fill(sortOrder.toString());
  await this.saveNotificationbtn.click();
  logger.info(`Enter Notification details : Name=${name}, Description=${description}, Sort Order=${sortOrder}`);

}

}

module.exports = { NotificationalertPage };
