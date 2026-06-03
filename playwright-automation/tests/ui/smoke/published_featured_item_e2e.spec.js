const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

let page, appPage, editorHomePage, headlineDetailsPage, subscriberSearchResultPage;
test.beforeEach(async ({ masterFixture }) => {
  page = masterFixture.page;
  appPage = masterFixture.appPage;
  editorHomePage = masterFixture.editorHomePage;
  headlineDetailsPage = masterFixture.headlineDetailsPage;
  subscriberSearchResultPage = masterFixture.subscriberSearchResultPage;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(5000);
});

test.describe('@smoke Featued content Publishing', () => {
  test(`Editor publishes an item and Subscriber can see it under Featued column`, async ({}) => {
    const parentPage = page;
    const editorHeadlineTitle = `Automation Featured Headline Title ${Date.now()}`;

    headlineDetailsPage = await editorHomePage.clickOnContent(CONSTANTS.CONTENTS.PRINT_CONTENT);

    await headlineDetailsPage.enterHeadLineTitle(editorHeadlineTitle);
    await headlineDetailsPage.enterByline(CONSTANTS.HEADLINES.BYLINE);
    await headlineDetailsPage.selectSource(CONSTANTS.HEADLINES.SOURCE_TORONTO_STAR);
    await headlineDetailsPage.enterSummary('Automation_Test_Summary');
    await headlineDetailsPage.selectTopStoriesOrCommentaryChecbox(CONSTANTS.HEADLINES.FEATURE_STORIES);
    await headlineDetailsPage.selectTag(CONSTANTS.HEADLINES.TAG_ADV);
    await headlineDetailsPage.clickOnSentimentButtonByText(CONSTANTS.HEADLINES.SENTIMENTS_2);
    await headlineDetailsPage.enterPrepTime('5');
    await headlineDetailsPage.publishHeadlines();
    expect(await headlineDetailsPage.verifyToastNotificationVisible(editorHeadlineTitle)).toBeTruthy();

    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();
    await appPage.logOut();

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);

    await subscriberSearchResultPage.clickOnSearchButton();

    await subscriberSearchResultPage.verifySearchResultPageLoaded();
    expect(
      await subscriberSearchResultPage.isPublishedHeadlinesPresent(editorHeadlineTitle),
    ).toBeTruthy();
    await appPage.logOutFromSubscriber();
  });
});
