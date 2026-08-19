import {
  type IAutomationProfileModel,
  type IAutomationRuleActionModel,
  type IAutomationStepModel,
} from '../interfaces';

export const actionTypeOptions = [
  { value: 'publish-content', label: 'Publish Content Item' },
  { value: 'unpublish-content', label: 'Unpublish Content Item' },
  { value: 'update-content-field', label: 'Update Content Field' },
  { value: 'add-action', label: 'Add Action' },
  { value: 'select-columnist', label: 'Select Columnist/Pundit' },
  { value: 'add-tags', label: 'Add Tags' },
  { value: 'add-sentiment', label: 'Add Sentiment' },
  { value: 'extract-data', label: 'Extract Data' },
  { value: 'create-content', label: 'Create Content' },
  { value: 'fetch-content', label: 'Fetch Content Collection' },
  { value: 'deduplicate', label: 'Deduplication' },
  { value: 'run-report', label: 'Publish Report' },
  { value: 'run-notification', label: 'Publish Notification' },
  { value: 'abort-step', label: 'Stop Remaining Actions' },
  { value: 'score-content', label: 'Score Content' },
  { value: 'select-top', label: 'Select Top Content' },
];

/** The action type whose target content field is configurable. */
export const UPDATE_CONTENT_FIELD_ACTION = 'update-content-field';

/**
 * Default prompt and confirmation statement for each action type.
 * Prompts are HTML because the action prompt editor (Quill) stores HTML.
 * Confirmation statements support tokens the automation engine resolves:
 * - `{field}` is replaced with the action's selected content field before matching.
 * - `{value}` is compiled into a capture group so data (e.g. a sentiment value or rewritten
 *   text) can be extracted from the LLM response. Statements without tokens are literal matches.
 * For large multiline values (HTML, XML, special characters) wrap `{value}` between start/end
 * marker lines so the capture is bounded (see 'update-content-field').
 */
export const actionTypeDefaults: Record<string, { prompt: string; confirmationStatement: string }> =
  {
    'publish-content': {
      prompt:
        '<p>Review the `content.headline`, `content.summary`, and `content.body`. If this story should be visible to subscribers respond with "[PUBLISH CONTENT]".</p>',
      confirmationStatement: '[PUBLISH CONTENT]',
    },
    'unpublish-content': {
      prompt:
        '<p>Review the `content.headline`, `content.summary`, and `content.body`. If this story should not be visible to subscribers respond with "[UNPUBLISH CONTENT]".</p>',
      confirmationStatement: '[UNPUBLISH CONTENT]',
    },
    'update-content-field': {
      prompt: [
        '<p>Review the content and generate an improved value for the {field} field. The new value may span multiple lines and include formatting (HTML, XML, special characters). Respond with the new value wrapped in markers exactly as follows:</p>',
        '<p>[UPDATE FIELD START:{field}]</p>',
        '<p>{value}</p>',
        '<p>[UPDATE FIELD END:{field}]</p>',
        '<p>Where {value} is the complete new value for the field.</p>',
      ].join(''),
      confirmationStatement: '[UPDATE FIELD START:{field}]\n{value}\n[UPDATE FIELD END:{field}]',
    },
    'add-action': {
      prompt:
        '<p>Review the `content.headline`, `content.summary`, and `content.body`. If this story qualifies for the selected content action respond with "[ADD ACTION]".</p>',
      confirmationStatement: '[ADD ACTION]',
    },
    'select-columnist': {
      prompt:
        '<p>Review the `content.byline` and `content.body`. If this story is an opinion column or editorial, identify the columnist/pundit and respond with "[COLUMNIST:{value}]" where {value} is the name of the columnist.</p>',
      confirmationStatement: '[COLUMNIST:{value}]',
    },
    'add-tags': {
      prompt:
        '<p>Review the `content.headline`, `content.summary`, and `content.body` and determine which tags apply to this story. Respond with "[TAGS:{value}]" where {value} is a comma-separated list of tag codes.</p>',
      confirmationStatement: '[TAGS:{value}]',
    },
    'add-sentiment': {
      prompt:
        '<p>Review the `content.headline`, `content.summary`, and `content.body` and generate a sentiment value from -5 to 5, where -5 is negative sentiment and 5 is positive sentiment. Base your evaluation on the perspective of the BC Government viewpoint. Respond with the following "[SENTIMENT:{value}]" where {value} is the sentiment value.</p>',
      confirmationStatement: '[SENTIMENT:{value}]',
    },
    // Extract Data and Create Content are driven by their grids (keys/values and the
    // property-to-key mapping), not the action prompt; the step prompt is used as an optional
    // preamble for any generate rows.
    'extract-data': {
      prompt: '',
      confirmationStatement: '',
    },
    'create-content': {
      prompt: '',
      confirmationStatement: '',
    },
    // Fetch Content Collection runs its filter and holds the results for later actions; it never
    // calls an LLM, so it has no prompt and no confirmation statement.
    'fetch-content': {
      prompt: '',
      confirmationStatement: '',
    },
    deduplicate: {
      prompt:
        '<p>Compare the following two news stories and determine whether they report the same underlying story. Compare the headline, the story text (the summary, or the body when there is no summary), and the published date. They may be duplicates even when the headline, byline, and body formatting differ (for example, papers owned by the same parent company often mirror each other\'s stories). If they are the same story respond with "[DUPLICATE]", otherwise respond with "[UNIQUE]".</p><p>Current story: {content}</p><p>Previously processed story: {previous}</p>',
      confirmationStatement: '[DUPLICATE]',
    },
    'run-report': {
      prompt:
        '<p>If the conditions to generate and send the report are met respond with "[RUN REPORT]".</p>',
      confirmationStatement: '[RUN REPORT]',
    },
    'run-notification': {
      prompt:
        '<p>If the conditions to send the notification are met respond with "[RUN NOTIFICATION]".</p>',
      confirmationStatement: '[RUN NOTIFICATION]',
    },
    'abort-step': {
      prompt:
        '<p>If the `content.body` is less than 100 words respond with "[IGNORE CONTENT]".</p>',
      confirmationStatement: '[IGNORE CONTENT]',
    },
    'score-content': {
      prompt:
        '<p>Review the `content.headline`, `content.summary`, and `content.body` and score how well this story meets the "{objective}" objective from 0 (not relevant) to 10 (exceptional). Respond with "[SCORE {objective}:{value}]" where {value} is the score.</p>',
      confirmationStatement: '[SCORE {objective}:{value}]',
    },
    'select-top': {
      prompt: [
        '<p>Review the following candidate stories and select the best ones for the "{objective}" objective. Each candidate includes its contentId, score, headline, source, and summary. Respond with "[SELECT {objective}:{value}]" where {value} is a comma-separated list of the selected contentId values.</p>',
        '<p>{candidates:{objective}}</p>',
      ].join(''),
      confirmationStatement: '[SELECT {objective}:{value}]',
    },
  };

/** The action type that adds a content action (Featured, Commentary, Top Story, Alert, ...). */
export const ADD_ACTION_ACTION = 'add-action';

/** The action type that scores a content item for an objective. */
export const SCORE_CONTENT_ACTION = 'score-content';

/** The action type that requests the reporting service run a report. */
export const RUN_REPORT_ACTION = 'run-report';

/** The action type that compares the current item against a prior action's processed content. */
export const DEDUPLICATION_ACTION = 'deduplicate';

/** The action type that runs a filter to gather a content collection for later actions. */
export const FETCH_CONTENT_ACTION = 'fetch-content';

/**
 * How a 'deduplicate' action compares the current item against its candidates.
 * - 'iterate' sends one LLM comparison per candidate (precise, one call each).
 * - 'batch' sends a digest of up to 'batchSize' candidates per comparison (far fewer calls,
 *   which is what makes comparing against a large fetched collection affordable).
 */
export const deduplicateModeOptions = [
  { value: 'iterate', label: 'One prompt per item' },
  { value: 'batch', label: 'Batch items into one prompt' },
];

/** Default number of candidates sent per prompt in batch mode. */
export const DEFAULT_DEDUPLICATE_BATCH_SIZE = 25;

/** Default number of content items a 'fetch-content' collection holds. */
export const DEFAULT_COLLECTION_MAX_ITEMS = 500;

/**
 * Batch-mode prompt and confirmation for 'deduplicate'. The confirmation must capture which
 * candidate matched, so it uses the {value} token to extract the duplicate's contentId — with
 * one prompt covering many candidates the response has to name the one it matched.
 */
export const deduplicateBatchDefaults = {
  prompt:
    '<p>Compare the current news story against each of the previously processed stories listed below and determine whether any of them report the same underlying story. Compare the headline, the story text, and the published date. They may be duplicates even when the headline, byline, and body formatting differ (for example, papers owned by the same parent company often mirror each other\'s stories). If the current story duplicates one of them respond with "[DUPLICATE:{value}]" where {value} is that story\'s contentId. If none of them are the same story respond with "[UNIQUE]".</p><p>Current story: {content}</p><p>Previously processed stories: {previous}</p>',
  confirmationStatement: '[DUPLICATE:{value}]',
};

/**
 * Action types eligible for 'Always run' (no LLM confirmation). Only types that do not
 * extract a value from the response can execute unconditionally.
 */
export const AUTO_EXECUTE_ACTION_TYPES = [
  'publish-content',
  'unpublish-content',
  'add-action',
  'run-report',
  'run-notification',
];

/** The action type that requests the notification service run a notification. */
export const RUN_NOTIFICATION_ACTION = 'run-notification';

/** The action type that selects the top scored content items for an objective. */
export const SELECT_TOP_ACTION = 'select-top';

/** Content fields the 'Update Content Field' action can target. */
export const contentFieldOptions = [
  { value: 'headline', label: 'Headline' },
  { value: 'byline', label: 'Byline' },
  { value: 'summary', label: 'Summary' },
  { value: 'body', label: 'Body' },
  { value: 'edition', label: 'Edition' },
  { value: 'section', label: 'Section' },
  { value: 'page', label: 'Page' },
];

/**
 * The default step prompt applied when the step target is 'content'.
 * Authored as HTML because the prompt editor (Quill) stores HTML; the automation engine converts
 * it to text before sending to the LLM. `{content}` is replaced with the content item JSON and
 * `{actions}` is replaced with the composed action prompts at runtime.
 */
export const defaultContentStepPrompt: string = [
  '<p>## Role</p>',
  '<p>You are an experienced news editor for the Media Monitoring Insights (MMI) service. You review news stories and decide which editorial actions should be performed on each story.</p>',
  '<p><br></p>',
  '<p>## Instructions</p>',
  '<ul>',
  '<li>Review the news story in the News Story section below.</li>',
  '<li>Evaluate each action in the Actions section against the story.</li>',
  '<li>Only confirm an action when its criteria are clearly met by the story.</li>',
  "<li>For each action you confirm, respond with that action's exact confirmation statement on its own line.</li>",
  '<li>If an action should not be performed, do not output its confirmation statement.</li>',
  '<li>Do not output anything else - no explanations, no formatting, no additional commentary.</li>',
  '</ul>',
  '<p><br></p>',
  '<p>## News Story Schema</p>',
  '<p>The news story is a JSON document from Elasticsearch with the following notable fields:</p>',
  '<ul>',
  '<li>id (number): unique identifier of the content item.</li>',
  '<li>status (string): workflow status; "Published" means visible to subscribers.</li>',
  '<li>contentType (string): the kind of content ("AudioVideo", "PrintContent", "Image", "Story").</li>',
  '<li>source (object): the publisher/outlet of the story. Fields: name (e.g. "Vancouver Sun"), code (unique source code, e.g. "SUN"), shortName. otherSource (string) holds the source code when no source record is linked.</li>',
  '<li>mediaType (object): the media category (e.g. Daily Print, Talk Radio, Online).</li>',
  '<li>series (object): the show/program, column, or publication series the story belongs to, when applicable. Fields: name; isOther indicates an ad-hoc series. otherSeries (string) holds a series name when no series record is linked.</li>',
  '<li>contributor (object): the columnist/pundit, when applicable.</li>',
  '<li>headline (string): the story headline.</li>',
  '<li>byline (string): the author(s).</li>',
  '<li>edition (string), section (string), page (string): print placement details.</li>',
  '<li>summary (string): the editor-written summary; may be empty.</li>',
  '<li>body (string): the full story text; may be empty for some media types.</li>',
  '<li>publishedOn (date-time): when the source published the story.</li>',
  '<li>postedOn (date-time): when the story was added to MMI.</li>',
  '<li>isHidden (boolean), isApproved (boolean), isPrivate (boolean): visibility and workflow flags.</li>',
  '<li>actions (array): editorial actions already applied to this story. Each item has: name (e.g. "Featured Story", "Top Story", "Commentary", "Alert"), value (the assigned value; may be empty), valueLabel and valueType (how to interpret value). The presence of an item means that action has already been applied - do not confirm an action that is already applied.</li>',
  '<li>topics (array): event-of-the-day topics assigned to the story. Each item has: name, score (importance score), topicType ("Issues" or "Proactive").</li>',
  '<li>tags (array): tags assigned to the story. Each item has: code (short unique code) and name.</li>',
  '<li>labels (array): key/value labels extracted or assigned.</li>',
  '<li>tonePools (array): sentiment ratings for the story. Each item has: name (the rater/tone pool) and value (integer from -5 very negative to +5 very positive; 0 is neutral).</li>',
  '<li>quotes (array): notable quotes extracted from the story. Each item has: statement (the quoted text) and byline (who said it).</li>',
  '</ul>',
  '<p><br></p>',
  '<p>## News Story</p>',
  '<p>Here is the news story you will review:</p>',
  '<pre>{content}</pre>',
  '<p><br></p>',
  '<p>## Actions</p>',
  '<p>Evaluate the following actions and respond with the confirmation statement for each action that should be performed on this story.</p>',
  '<p>{actions}</p>',
].join('');

/**
 * The prompt section injected when the step has a filter that runs as a separate Elasticsearch
 * query ('Apply filter to profile content' is off). `{results}` is replaced with the step filter
 * search results at runtime.
 */
export const defaultResultsPromptSection: string = [
  '<p>## Related Search Results</p>',
  '<p>The step filter returned the following related search results. These are separate from the news story above and are provided as supporting context - use them when evaluating the actions (for example to compare coverage, detect duplicates, or judge prominence). Each result is a news story JSON document that follows the same schema described above.</p>',
  '<pre>{results}</pre>',
  '<p><br></p>',
].join('');

const NEWS_STORY_SECTION =
  '<p>## News Story</p><p>Here is the news story you will review:</p><pre>{content}</pre><p><br></p>';
const ACTIONS_MARKER = '<p>## Actions</p>';

/**
 * The sections prefixed to an action prompt when the step uses chat completions: each action is
 * its own user message, so it must carry the news story and its instructions itself (the step
 * prompt is the system prompt and excludes them).
 */
export const CHAT_ACTION_PROMPT_PREFIX = [
  '<p>## News Story</p>',
  '<p>Here is the news story you will review:</p>',
  '<pre>{content}</pre>',
  '<p><br></p>',
  '<p>## Actions</p>',
  '<p>Evaluate the following action and respond with the confirmation statement that should be performed on this story.</p>',
  '<p><br></p>',
].join('');

/** The previous chat action prefix; kept only so default detection recognizes older prompts. */
export const LEGACY_CHAT_ACTION_PROMPT_PREFIX = `${NEWS_STORY_SECTION}${ACTIONS_MARKER}`;

/**
 * Build the default prompt for an action. When the step uses chat completions the default
 * includes the News Story and Actions sections (the step/system prompt excludes them).
 */
export const buildDefaultActionPrompt = (
  actionType: string,
  useChatCompletions: boolean,
): string => {
  const base = actionTypeDefaults[actionType]?.prompt ?? '';
  return useChatCompletions && base ? `${CHAT_ACTION_PROMPT_PREFIX}${base}` : base;
};

/**
 * The default step prompt (system prompt) for steps using chat completions: role, instructions,
 * and the news story schema. The News Story and Actions sections are carried by each action's
 * own prompt instead.
 */
export const chatStepSystemPrompt: string = [
  '<p>## Role</p>',
  '<p>You are an experienced news editor for the Media Monitoring Insights (MMI) service. You review news stories and decide which editorial actions should be performed on each story.</p>',
  '<p><br></p>',
  '<p>## Instructions</p>',
  '<ul>',
  '<li>Review the news story in the News Story section below.</li>',
  '<li>Evaluate each action in the Actions section against the story.</li>',
  '<li>Only confirm an action when its criteria are clearly met by the story.</li>',
  "<li>For each action you confirm, respond with that action's exact confirmation statement on its own line.</li>",
  '<li>If an action should not be performed, do not output its confirmation statement.</li>',
  '<li>Do not output anything else - no explanations, no formatting, no additional commentary.</li>',
  '</ul>',
  '<p><br></p>',
  '<p>## News Story Schema</p>',
  '<p>The news story is a JSON document from Elasticsearch with the following notable fields:</p>',
  '<ul>',
  '<li>id (number): unique identifier of the content item.</li>',
  '<li>status (string): workflow status; "Published" means visible to subscribers.</li>',
  '<li>contentType (string): the kind of content ("AudioVideo", "PrintContent", "Image", "Story").</li>',
  '<li>source (object): the publisher/outlet of the story. Fields: name (e.g. "Vancouver Sun"), code (unique source code, e.g. "SUN"), shortName. otherSource (string) holds the source code when no source record is linked.</li>',
  '<li>mediaType (object): the media category (e.g. Daily Print, Talk Radio, Online).</li>',
  '<li>series (object): the show/program, column, or publication series the story belongs to, when applicable. Fields: name; isOther indicates an ad-hoc series. otherSeries (string) holds a series name when no series record is linked.</li>',
  '<li>contributor (object): the columnist/pundit, when applicable.</li>',
  '<li>headline (string): the story headline.</li>',
  '<li>byline (string): the author(s).</li>',
  '<li>edition (string), section (string), page (string): print placement details.</li>',
  '<li>summary (string): the editor-written summary; may be empty.</li>',
  '<li>body (string): the full story text; may be empty for some media types.</li>',
  '<li>publishedOn (date-time): when the source published the story.</li>',
  '<li>postedOn (date-time): when the story was added to MMI.</li>',
  '<li>isHidden (boolean), isApproved (boolean), isPrivate (boolean): visibility and workflow flags.</li>',
  '<li>actions (array): editorial actions already applied to this story. Each item has: name (e.g. "Featured Story", "Top Story", "Commentary", "Alert"), value (the assigned value; may be empty), valueLabel and valueType (how to interpret value). The presence of an item means that action has already been applied - do not confirm an action that is already applied.</li>',
  '<li>topics (array): event-of-the-day topics assigned to the story. Each item has: name, score (importance score), topicType ("Issues" or "Proactive").</li>',
  '<li>tags (array): tags assigned to the story. Each item has: code (short unique code) and name.</li>',
  '<li>labels (array): key/value labels extracted or assigned.</li>',
  '<li>tonePools (array): sentiment ratings for the story. Each item has: name (the rater/tone pool) and value (integer from -5 very negative to +5 very positive; 0 is neutral).</li>',
  '<li>quotes (array): notable quotes extracted from the story. Each item has: statement (the quoted text) and byline (who said it).</li>',
  '</ul>',
  '<p><br></p>',
  '<p>## News Story</p>',
  '<p>Here is the news story you will review:</p>',
  '<pre>{content}</pre>',
].join('');

/**
 * Build the default step prompt.
 * When the step has an enrichment filter (a filter that is not applied as a gate to the profile
 * content), a Related Search Results section with the `{results}` token is included before the
 * Actions section.
 * Steps with a 'start', 'end', or 'none' target do not iterate content, so their default prompt
 * excludes the News Story section (the `{content}` token has no value for those targets).
 * Steps using chat completions always use the chat system prompt (role, instructions, schema) -
 * each action's own prompt carries the News Story and Actions sections.
 */
export const buildDefaultContentStepPrompt = (
  includeResults: boolean,
  target: IAutomationStepModel['target'] = 'content',
  useChatCompletions: boolean = false,
): string => {
  if (useChatCompletions) return chatStepSystemPrompt;
  let prompt = defaultContentStepPrompt;
  if (target !== 'content') prompt = prompt.replace(NEWS_STORY_SECTION, '');
  if (includeResults)
    prompt = prompt.replace(ACTIONS_MARKER, `${defaultResultsPromptSection}${ACTIONS_MARKER}`);
  return prompt;
};

const defaultAction: IAutomationRuleActionModel = {
  name: '',
  prompt: actionTypeDefaults[actionTypeOptions[0].value]?.prompt ?? '',
  actionType: actionTypeOptions[0].value,
  maxCalls: null,
  confirmationStatement:
    actionTypeDefaults[actionTypeOptions[0].value]?.confirmationStatement ?? '',
  isEnabled: true,
  contentField: null,
  contentActionId: null,
  filterId: null,
  objective: null,
  autoExecute: false,
  abortIfNoConfirmation: false,
};

const defaultStep: IAutomationStepModel = {
  id: 0,
  name: 'Default Step',
  description: '',
  prompt: defaultContentStepPrompt,
  target: 'content',
  filterId: undefined,
  applyToAutomationFilter: false,
  iterateStepFilter: false,
  sendSeparatePrompts: false,
  useChatCompletions: false,
  priority: 0,
  isEnabled: true,
  actions: [],
};

export const defaultAutomationProfile: IAutomationProfileModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  schemaVersion: 1,
  filterId: undefined,
  llmId: undefined,
  schedules: [],
  steps: [],
};

export const createDefaultAction = (): IAutomationRuleActionModel => ({
  ...defaultAction,
});

export const createDefaultStep = (): IAutomationStepModel => ({
  ...defaultStep,
  actions: [],
});
