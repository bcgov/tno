const BasePage = require('./base/BasePage');
const CONSTANTS = require('../utils/constants');
const logger = require('../utils/logger');
const { getFilePath } = require('../utils/fileUpload.util');
const { expect } = require('@playwright/test');

class ReportSubscriberSidePage extends BasePage {
  constructor(page) {
    super(page);

    this.myReportsLink = page.getByText('My Reports')
    this.newReportButton = page.getByRole('button', { name: 'New Report' })
    this.reportNameInput = page.getByRole('textbox', { name: 'Report Name: *' })
    this.reportDescriptionInput = page.getByRole('textbox', { name: 'Description:' })
    this.reportSubjectLineEmailInput = page.getByRole('textbox', { name: 'Email subject line: *' })
    this.reportSaveButton = page.getByRole('button', { name: 'Save report' })
    this.clickReportLink = page.getByText('My Reports', { exact: true })
    this.deleteButton = page.locator('.sc-fqkwJk > .sc-eyvHYj > svg')
    this.confirmDeleteButton = page.getByText('Yes, Remove It')
    this.settingsTab = page.getByRole('main').getByText('Settings')
    this.contentSectionTab = page.locator('div').filter({ hasText: /^Content$/ }).nth(1)
    this.previewAndSendTab = page.locator('div').filter({ hasText: /^Preview & Send$/ }).nth(1)
    this.historyTab = page.locator('div').filter({ hasText: /^History$/ }).nth(1)
    this.cancelButton = page.getByRole('button', { name: 'Cancel' })
    this.templateTab = page.getByText('Template')
    this.dataSourcesTab = page.getByText('Data Sources')
    this.preferencesTab = page.getByText('Preferences')
    this.subscribersTab = page.getByText('Subscribers')
    this.scheduleTab = page.getByText('Schedule')
    this.contentCurateStories = page.getByText('Show folder sections')
    this.quickSort = page.getByText('Quick Sort')
    this.executiveSummary = page.getByText('Executive Summary')
    this.tableOfContents = page.getByRole('button', { name: 'Table of Contents' })
    this.aiTab = page.getByRole('button', { name: 'AI' })
    this.templateStories = page.getByRole('button', { name: 'Stories' })
    this.templateMediaAnalytics = page.getByRole('button', { name: 'Media Analytics' })
    this.templateText = page.getByRole('button', { name: 'Text' })
    this.templateFrontPageImages = page.getByRole('button', { name: 'Front Page Images' })
    this.templateImage= page.getByRole('button', { name: 'Image', exact: true })
    this.templateData = page.getByRole('button', { name: 'Data' })

  }


async clickOnTemplateFrontPageImages() {
    await this.click(this.templateFrontPageImages);
    logger.info('Clicked on Front Page Images template option!!');
  }
async clickOnTemplateImage() {
    await this.click(this.templateImage);
    logger.info('Clicked on Image template option!!');
  }
async clickOnTemplateData() {
    await this.click(this.templateData);
    logger.info('Clicked on Data template option!!');
  }



async clickOnTemplateStories() {
    await this.click(this.templateStories);
    logger.info('Clicked on Stories template option!!');
  }

async clickOnTemplateMediaAnalytics() {
    await this.click(this.templateMediaAnalytics);
    logger.info('Clicked on Media Analytics template option!!');
  }

async clickOnTemplateText() {
    await this.click(this.templateText);
    logger.info('Clicked on Text template option!!');
  }





async clickOnTableOfContents() {
    await this.click(this.tableOfContents);
    logger.info('Clicked on Table of Contents option!!');
  }

  async clickOnAiTab() {
    await this.click(this.aiTab);
    logger.info('Clicked on AI option!!');
  }






async clickOnExecutiveSummary() {
    await this.click(this.executiveSummary);
    logger.info('Clicked on Executive Summary option!!');
  }

  async clickOnqQuickSort() {
    await this.click(this.quickSort);
    logger.info('Clicked on Quick Sort option!!');
  }


async validatecontentCurateStoriesText() {
    const isVisible = await this.contentCurateStories.isVisible();
    expect(isVisible).toBeTruthy();
    logger.info('Content Curate Stories text is visible on Content Section tab!!');
  }



async clickOnSubscribersTab() {
    await this.click(this.subscribersTab);
    logger.info('Clicked on Subscribers tab!!');
  }

  async clickOnScheduleTab() {
    await this.click(this.scheduleTab);
    logger.info('Clicked on Schedule tab!!');
  }


async clickOnPreferencesTab() {
    await this.click(this.preferencesTab);
    logger.info('Clicked on Preferences tab!!');
  }




  async clickOnDataSourcesTab() {
    await this.click(this.dataSourcesTab);
    logger.info('Clicked on Data Sources tab!!');
  }


async clickOnTemplateTab() {
    await this.click(this.templateTab);
    logger.info('Clicked on Template tab!!');
  }


  async clickOnCancelButton() {
    await this.click(this.cancelButton);
    logger.info('Clicked on Cancel button!!');
  }
  
async clickOnHistoryTab() {
    await this.click(this.historyTab);
    logger.info('Clicked on History tab!!');
  }

async clickOnSettingsTab() {
    await this.click(this.settingsTab);
    logger.info('Clicked on Settings tab!!');
  }

async clickOnContentSectionTab() {
    await this.click(this.contentSectionTab);
    logger.info('Clicked on Content Section tab!!');
  }

async clickOnPreviewAndSendTab() {
    await this.click(this.previewAndSendTab);
    logger.info('Clicked on Preview & Send tab!!');
  }





async clickOnDelete() {
    await this.click(this.deleteButton);
    await this.click(this.confirmDeleteButton);
    logger.info('Clicked on Delete button!!');
  }

async clickOnMyreportsLink() {
    await this.click(this.myReportsLink);
    logger.info('Clicked on My Reports link!!');
  }
async clickOnVisibledata() {
    await this.click(this.clickReportLink);
    logger.info('Clicked on Report link!!');
  }

async clickOnNewReportButton() {
    await this.click(this.newReportButton);
    logger.info('Clicked on New Report button!!');
  }


async enterReportName(reportName) {
    await this.type(this.reportNameInput, reportName);
    logger.info(`Entered report name: ${reportName}`);
  }

async enterReportDescription(description) {
    await this.type(this.reportDescriptionInput, description);
    logger.info(`Entered report description: ${description}`);
  }

async enterReportSubjectLineEmail(subjectLine) {
    await this.type(this.reportSubjectLineEmailInput, subjectLine);
    logger.info(`Entered report subject line: ${subjectLine}`);
  }

async clickOnReportSaveButton() {
    await this.click(this.reportSaveButton);
    logger.info('Clicked on Save Report button!!');
  }







}
module.exports = { ReportSubscriberSidePage };
