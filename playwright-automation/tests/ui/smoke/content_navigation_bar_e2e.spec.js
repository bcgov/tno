const { test, expect } = require('../../../fixtures/ui-fixture');
const DataLoader = require('../../../utils/dataLoader');
const CONSTANTS = require('../../../utils/constants');

const testData = DataLoader.loadJSON(`test-data/${process.env.ENV_NAME}/loginData.json`);
const testApp = process.env.APP_NAME;
const editorUrl = testData[testApp]['editor']['url'];

let page, appPage, editorHomePage, headlineDetailsPage, subscriberSearchResultPage, previewPage;

test.beforeEach(async ({ masterFixture }) => {
  page = masterFixture.page;
  appPage = masterFixture.appPage;
  editorHomePage = masterFixture.editorHomePage;
  headlineDetailsPage = masterFixture.headlineDetailsPage;
  subscriberSearchResultPage = masterFixture.subscriberSearchResultPage;
  previewPage = masterFixture.previewPage;
  await appPage.navigateToUrl(editorUrl);
  await appPage.hardWait(2000);
});

test.describe('@smoke Content Paper workflow', () => {
  test(`Verify adding headlines to Top Story and open publish pop-up via Paper section`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.PAPERS);
    expect(
      await editorHomePage.isAdvanceSearchTextBoxForPapersContentPresent(
        CONSTANTS.HEADLINES.TOP_STORY,
      ),
    ).toBe(true);
    expect(
      await editorHomePage.isAdvanceSearchTextBoxForPapersContentPresent(
        CONSTANTS.HEADLINES.COMMENTARY,
      ),
    ).toBe(true);
    expect(
      await editorHomePage.isAdvanceSearchTextBoxForPapersContentPresent(
        CONSTANTS.HEADLINES.FEATURE_STORIES,
      ),
    ).toBe(true);
    expect(
      await editorHomePage.isAdvanceSearchTextBoxForPapersContentPresent(
        CONSTANTS.HEADLINES.PUBLISHED,
      ),
    ).toBe(true);

    expect(await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.HIDE)).toBe(
      false,
    );
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.ADD_TO_TOP_STORY),
    ).toBe(false);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(
        CONSTANTS.BUTTONS.ADD_TO_FEATURED_STOREIS,
      ),
    ).toBe(false);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.ADD_TO_COMMENTARY),
    ).toBe(false);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.PUBLISH_SELECTED),
    ).toBe(false);

    const headlineTitle = await editorHomePage.getHeadlinesTitleByRowNumberOnPapersEditorGrid(6);
    await editorHomePage.selectOnHeadlinesCheckBoxByRowNumber(6);
    // expect(await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.HIDE)).toBe(true);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.ADD_TO_TOP_STORY),
    ).toBe(true);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(
        CONSTANTS.BUTTONS.ADD_TO_FEATURED_STOREIS,
      ),
    ).toBe(true);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.ADD_TO_COMMENTARY),
    ).toBe(true);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.PUBLISH_SELECTED),
    ).toBe(true);

    const beforeTopStoryCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.TOP_STORIES,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.ADD_TO_TOP_STORY,
    );
    const afterTopStoryCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.TOP_STORIES,
    );
    expect(+afterTopStoryCount).toBe(+beforeTopStoryCount + 1);

    const beforePublishedCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.PUBLISHED,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.PUBLISH_SELECTED,
    );
    const afterPublishedCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.PUBLISHED,
    );
    expect(+afterPublishedCount).toBe(+beforePublishedCount + 1);

    const parentPage = page;
    previewPage = await editorHomePage.previewMorningReport();
    expect(await previewPage.verifyPreviewReportTitle()).toBe('MMI MORNING REPORT');
    expect(await previewPage.verifyPublishedHeadlineTitleOnPreviewPage(headlineTitle)).toBe(true);

    await previewPage.closePage();
    await parentPage.bringToFront();

    await editorHomePage.clickToSendTopStories(CONSTANTS.BUTTONS.CANCEL);

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.TOP_STORY);
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.REMOVE_FROM_TOP_STORY,
    );
    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.TOP_STORY);

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.PUBLISHED);
    headlineDetailsPage = await editorHomePage.clickOnHeadlinesByGivenTitle(headlineTitle);
    await headlineDetailsPage.unPublishHeadlines();
    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await appPage.logOut();
  });

  test(`Verify adding headlines to feature Story and open publish pop-up via Paper section`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.PAPERS);

    const headlineTitle = await editorHomePage.getHeadlinesTitleByRowNumberOnPapersEditorGrid(7);
    await editorHomePage.selectOnHeadlinesCheckBoxByRowNumber(7);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(
        CONSTANTS.BUTTONS.ADD_TO_FEATURED_STOREIS,
      ),
    ).toBe(true);

    const beforeFeatureStoryCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.FEATURE_STORIES,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.ADD_TO_FEATURED_STOREIS,
    );
    const afterFeatureStoryCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.FEATURE_STORIES,
    );
    expect(+afterFeatureStoryCount).toBe(+beforeFeatureStoryCount + 1);

    const beforePublishedCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.PUBLISHED,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.PUBLISH_SELECTED,
    );
    const afterPublishedCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.PUBLISHED,
    );
    expect(+afterPublishedCount).toBe(+beforePublishedCount + 1);

    const parentPage = page;
    previewPage = await editorHomePage.previewMorningReport();
    expect(await previewPage.verifyPreviewReportTitle()).toBe('MMI MORNING REPORT');
    expect(await previewPage.verifyPublishedHeadlineTitleOnPreviewPage(headlineTitle)).toBe(true);

    await previewPage.closePage();
    await parentPage.bringToFront();

    await editorHomePage.clickToSendTopStories(CONSTANTS.BUTTONS.CANCEL);

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(
      CONSTANTS.HEADLINES.FEATURE_STORIES,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.REMOVE_FROM_FEATURE_STORIES,
    );
    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(
      CONSTANTS.HEADLINES.FEATURE_STORIES,
    );

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.PUBLISHED);
    headlineDetailsPage = await editorHomePage.clickOnHeadlinesByGivenTitle(headlineTitle);
    await headlineDetailsPage.unPublishHeadlines();
    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await appPage.logOut();
  });

  test(`Verify adding headlines to Commentary and open publish pop-up via Paper section`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.PAPERS);

    const headlineTitle = await editorHomePage.getHeadlinesTitleByRowNumberOnPapersEditorGrid(7);
    await editorHomePage.selectOnHeadlinesCheckBoxByRowNumber(7);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.ADD_TO_COMMENTARY),
    ).toBe(true);

    const beforeCommentaryCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.COMMENTARY,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.ADD_TO_COMMENTARY,
    );
    const afterCommentaryCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.COMMENTARY,
    );
    expect(+afterCommentaryCount).toBe(+beforeCommentaryCount + 1);

    const beforePublishedCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.PUBLISHED,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.PUBLISH_SELECTED,
    );
    const afterPublishedCount = await editorHomePage.papersTotalCountForGivenHeadlineType(
      CONSTANTS.HEADLINES.PUBLISHED,
    );
    expect(+afterPublishedCount).toBe(+beforePublishedCount + 1);

    const parentPage = page;
    previewPage = await editorHomePage.previewMorningReport();
    expect(await previewPage.verifyPreviewReportTitle()).toBe('MMI MORNING REPORT');
    expect(await previewPage.verifyPublishedHeadlineTitleOnPreviewPage(headlineTitle)).toBe(true);

    await previewPage.closePage();
    await parentPage.bringToFront();

    await editorHomePage.clickToSendTopStories(CONSTANTS.BUTTONS.CANCEL);

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.COMMENTARY);
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.REMOVE_FROM_COMMENTARY,
    );
    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.COMMENTARY);

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.PUBLISHED);
    headlineDetailsPage = await editorHomePage.clickOnHeadlinesByGivenTitle(headlineTitle);
    await headlineDetailsPage.unPublishHeadlines();
    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await appPage.logOut();
  });

  test(`Verify adding headlines to Top Story and publish to subscriber`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.PAPERS);
    expect(
      await editorHomePage.isAdvanceSearchTextBoxForPapersContentPresent(
        CONSTANTS.HEADLINES.TOP_STORY,
      ),
    ).toBe(true);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.ADD_TO_TOP_STORY),
    ).toBe(false);

    const headlineTitle = await editorHomePage.getHeadlinesTitleByRowNumberOnPapersEditorGrid(8);
    await editorHomePage.selectOnHeadlinesCheckBoxByRowNumber(8);
    expect(
      await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.ADD_TO_TOP_STORY),
    ).toBe(true);

    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.ADD_TO_TOP_STORY,
    );
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.PUBLISH_SELECTED,
    );

    const parentPage = page;
    previewPage = await editorHomePage.previewMorningReport();
    expect(await previewPage.verifyPreviewReportTitle()).toBe('MMI MORNING REPORT');
    expect(await previewPage.verifyPublishedHeadlineTitleOnPreviewPage(headlineTitle)).toBe(true);

    await previewPage.closePage();
    await parentPage.bringToFront();

    await editorHomePage.clickToSendTopStories(CONSTANTS.BUTTONS.SAVE);
    await appPage.logOut();

    await appPage.navigateToSubscriberURL();
    await appPage.loginAsSubscriber(process.env.sub_username, process.env.sub_password);

    await subscriberSearchResultPage.clickOnSearchButton();
    await subscriberSearchResultPage.verifySearchResultPageLoaded();

    expect(await subscriberSearchResultPage.isPublishedHeadlinesPresent(headlineTitle)).toBe(true);
    await appPage.logOutFromSubscriber();

    await appPage.navigateToUrl(editorUrl);
    await editorHomePage.verifyEditorHomePageLoaded();
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.PAPERS);

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.TOP_STORY);
    await editorHomePage.selectOnHeadlinesCheckBoxByRowNumber(1);
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(
      CONSTANTS.BUTTONS.REMOVE_FROM_TOP_STORY,
    );
    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.TOP_STORY);

    await editorHomePage.selectAdvanceSearchTextBoxForPapersContent(CONSTANTS.HEADLINES.PUBLISHED);
    headlineDetailsPage = await editorHomePage.clickOnHeadlinesByGivenTitle(headlineTitle);
    await headlineDetailsPage.unPublishHeadlines();
    await headlineDetailsPage.closePage();
    await parentPage.bringToFront();

    await appPage.logOut();
  });

  test(`Verify Transcript queue filtering`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.TRANSCRIPT_QUEUE);

    await editorHomePage.selectAdvanceSearchTextBoxForTranscriptQueueContent(
      CONSTANTS.HEADLINES.READY_FOR_REVIEW,
    );
    await editorHomePage.verifyAllRecordsHaveSelectedStatusOnTranscriptQueueGrid(
      CONSTANTS.HEADLINES.READY_FOR_REVIEW,
    );
    await editorHomePage.selectAdvanceSearchTextBoxForTranscriptQueueContent(
      CONSTANTS.HEADLINES.READY_FOR_REVIEW,
    );

    await editorHomePage.selectAdvanceSearchTextBoxForTranscriptQueueContent(
      CONSTANTS.HEADLINES.IN_PROGRESS,
    );
    await editorHomePage.verifyAllRecordsHaveSelectedStatusOnTranscriptQueueGrid(
      CONSTANTS.HEADLINES.IN_PROGRESS,
    );
    await editorHomePage.selectAdvanceSearchTextBoxForTranscriptQueueContent(
      CONSTANTS.HEADLINES.IN_PROGRESS,
    );

    await editorHomePage.selectAdvanceSearchTextBoxForTranscriptQueueContent(
      CONSTANTS.HEADLINES.FAILED,
    );
    await editorHomePage.verifyAllRecordsHaveSelectedStatusOnTranscriptQueueGrid(
      CONSTANTS.HEADLINES.FAILED,
    );

    await appPage.logOut();
  });

  test(`Verify Hide functionality on Content page`, async ({}) => {
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.NAVIGATIONMENU.CONTENT);
    await appPage.clickOnMenuAndSubNavigationMenuLink(CONSTANTS.CONTENT_SUBMENU.PAPERS);

    const headlineTitleBeforeHide = await editorHomePage.getHeadlinesTitleByRowNumberOnPapersEditorGrid(7);
    await editorHomePage.selectOnHeadlinesCheckBoxByRowNumber(7);
    expect(await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.HIDE)).toBe(true);   

    await editorHomePage.clickButtonForPapersContentOnEditorGrid(CONSTANTS.BUTTONS.HIDE);

    await editorHomePage.selectSeeHiddenOnlyFilterContent();
    const headlineTitleAfterHide = await editorHomePage.getHeadlinesTitleByRowNumberOnPapersEditorGrid(1);

    expect(headlineTitleBeforeHide).toBe(headlineTitleAfterHide);

    expect(await editorHomePage.isButtonEnabledOnPapersEditorGrid(CONSTANTS.BUTTONS.UNHIDE)).toBe(true);   
    await editorHomePage.clickButtonForPapersContentOnEditorGrid(CONSTANTS.BUTTONS.UNHIDE);

    await appPage.logOut();
  });
});
