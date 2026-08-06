export interface IAutomationRuleActionModel {
  /** Primary key; 0 until saved. Round-tripped so saves keep action identities stable. */
  id?: number;
  name: string;
  prompt: string;
  actionType: string;
  maxCalls?: number | null;
  confirmationStatement: string;
  isEnabled: boolean;
  /** The content field to update; only used when actionType is 'update-content-field'. */
  contentField?: string | null;
  /** The content action to add (FK to Action); used when actionType is 'add-action' or 'select-top'. */
  contentActionId?: number | null;
  /** The report to run (FK to Report); only used when actionType is 'run-report'. */
  reportId?: number | null;
  /** The notification to run (FK to Notification); only used when actionType is 'run-notification'. */
  notificationId?: number | null;
  /** A prior action (FK to AutomationAction) whose processed content is compared against the current item; only used when actionType is 'deduplicate'. */
  priorActionId?: number | null;
  /** Whether the action executes unconditionally without LLM confirmation (value-less action types only). */
  autoExecute?: boolean;
  /** Whether to abort the remaining actions on the step when this action does not receive its confirmation (e.g. a 'publish' that did not happen). */
  abortIfNoConfirmation?: boolean;
  /** Optional LLM for this action's prompt; only used when the step sends separate prompts. */
  llmId?: number;
  /** The scoring objective key; links 'score-content' actions to their 'select-top' action. */
  objective?: string | null;
  /** Which content the action operates on: 'original' (or undefined) targets the iterated item;
   * otherwise the identifier of a content item created earlier in the same step by a
   * 'create-content' action. Only honoured when the step sends separate prompts. */
  worksOn?: string | null;
  /** For 'create-content': the identifier later actions reference via worksOn (e.g. 'c1'). */
  createIdentifier?: string | null;
  /** For 'create-content': clone the iterated content item as the starting point before applying
   * the extracted data and prompt values. */
  createClone?: boolean;
  /** Action-type specific configuration (e.g. create-content 'overrides' and 'requireKey'). */
  settings?: Record<string, any> | null;
}

export interface IAutomationStepModel {
  id: number;
  name: string;
  description: string;
  prompt: string;
  target: 'none' | 'content' | 'start' | 'end';
  filterId?: number;
  applyToAutomationFilter: boolean;
  /** Whether the step iterates over the step filter's results ('start'/'end' targets only). */
  iterateStepFilter: boolean;
  /** Optional LLM used for this step's prompts instead of the profile's LLM. */
  llmId?: number;
  /** Whether each action sends its own prompt (step prompt + that action's prompt). */
  sendSeparatePrompts: boolean;
  /** Whether the step runs as a chat conversation: the step prompt is the system prompt and each action is its own user message sharing the conversation context. */
  useChatCompletions: boolean;
  priority: number;
  isEnabled: boolean;
  actions: IAutomationRuleActionModel[];
}

export interface IAutomationScheduleModel {
  /** The event schedule id (0 when new). */
  id: number;
  /** A name to identify the schedule. */
  name: string;
  isEnabled: boolean;
  /** Time of day to run at (or after), 'HH:mm:ss'. */
  startAt?: string | null;
  /** ScheduleWeekDay flag values; empty runs every day. */
  runOnWeekDays: number[];
}

export interface IAutomationProfileModel {
  id: number;
  name: string;
  description: string;
  isEnabled: boolean;
  schemaVersion: number;
  filterId?: number;
  llmId?: number;
  schedules: IAutomationScheduleModel[];
  steps: IAutomationStepModel[];
}

export interface IAutomationLegacyProfileModel extends IAutomationProfileModel {
  rules?: IAutomationStepModel[];
  llmId?: number;
}

export interface IAutomationRunRequestModel {
  note?: string;
}

/** A single message in a debugging conversation. */
export interface IAutomationDebugMessageModel {
  role: string;
  content: string;
}

/** A request to ask the profile's LLM why a content item was (or was not) acted upon. */
export interface IAutomationDebugRequestModel {
  contentId: number;
  question: string;
  /** The conversation returned from the previous response; empty starts a new chat. */
  messages?: IAutomationDebugMessageModel[];
}

/** The LLM's answer to a debugging turn, plus the full conversation to continue it. */
export interface IAutomationDebugResultModel {
  contentId: number;
  runId?: number;
  prompt: string;
  answer: string;
  messages: IAutomationDebugMessageModel[];
}

export interface IAutomationRunModel {
  id: number;
  profileId?: number;
  status?: string | number;
  trigger?: string;
  note?: string;
  startedOn?: string;
  completedOn?: string;
}

export interface IAutomationRunResponseModel {
  stepId: number;
  stepName: string;
  actionName?: string | null;
  contentId?: number | null;
  /** The prompt sent to the LLM; only present when the service records prompts. */
  prompt?: string | null;
  response: string;
}

export interface IAutomationRunDiffModel {
  run: IAutomationRunModel;
  changes: unknown[];
  stepHits: unknown[];
  responses: IAutomationRunResponseModel[];
}
