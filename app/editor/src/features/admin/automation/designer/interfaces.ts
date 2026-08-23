/** One prompt library entry. Stored as a bare string when it has no description (legacy form);
 * the parser normalizes both shapes to this object. */
export interface IAutomationPromptEntry {
  text: string;
  /** What the prompt is for, shown in the library table. */
  description?: string | null;
}

/** The v2 profile definition document, stored as JSON on the profile (schemaVersion >= 2). */
export interface IAutomationDefinition {
  /** The prompt library: named entries shared via prompt refs. */
  prompts: Record<string, IAutomationPromptEntry>;
  steps: IAutomationStep[];
}

export interface IAutomationStep {
  name: string;
  description?: string;
  /** Optional group label; the steps grid bands consecutive steps sharing it. The engine and
   * validator ignore it (unknown document properties round-trip untouched). */
  group?: string;
  /** Lifecycle phase: 'init' (once, first), 'process' (per item), 'complete' (once, last). */
  phase: 'init' | 'process' | 'complete';
  isEnabled: boolean;
  /** Where a process step's content comes from; required for 'process', absent otherwise. */
  source?: IAutomationSource;
  llmId?: number | null;
  analyses: IAutomationAnalysis[];
  actions: IAutomationAction[];
}

export interface IAutomationSource {
  /** 'filter' (the step runs its own search) or 'collection' (a named run collection;
   * content enters a run through 'search' actions). */
  from: 'filter' | 'collection';
  filter?: number | null;
  collection?: string | null;
  /** Gate filter ids: only items matching every one are processed. */
  include?: number[];
  /** Gate filter ids: items matching any are skipped. */
  exclude?: number[];
  /** Digest field projection override. */
  fields?: string[] | null;
  max?: number | null;
}

export interface IAutomationAnalysis {
  name: string;
  prompt: IAutomationPrompt;
  /** Continue an earlier analysis as a conversation (the model sees the earlier exchange). */
  chain?: string | null;
  /** Result shape: key -> type spec ('string', 'string?', 'string[]', 'bool', 'int', 'int(a..b)'). */
  returns: Record<string, string>;
  /** Raw mode: keep the response as text; actions gate on it with confirmation statements. */
  raw?: boolean;
  llmId?: number | null;
}

export interface IAutomationPrompt {
  /** A prompt library entry name. */
  ref?: string | null;
  /** Inline prompt text (used alone). */
  text?: string | null;
  /** Text layered onto the referenced library entry. */
  override?: string | null;
}

/** A declarative gate: exactly one shape — a leaf (field/op/value), a combinator (all/any/not),
 * or an analysis-result gate (from: 'analysisName.key'). */
export interface IAutomationCondition {
  field?: string | null;
  op?: string | null;
  value?: unknown;
  all?: IAutomationCondition[];
  any?: IAutomationCondition[];
  not?: IAutomationCondition | null;
  from?: string | null;
}

/** Where an action's value comes from — a fixed source, never an expression. */
export interface IAutomationValueSource {
  /** 'analysisName.key' or 'content.field'. */
  from?: string | null;
  literal?: unknown;
  /** Token template, e.g. 'DIGEST: {content.headline}'. */
  template?: string | null;
}

export interface IAutomationAction {
  type: string;
  name?: string | null;
  isEnabled: boolean;
  /** Property condition / analysis gate; evaluated before any prompt is sent. */
  when?: IAutomationCondition | null;
  /** Confirmation statement matched against a raw analysis response ({value} capture). */
  confirm?: string | null;
  /** The analysis whose response 'confirm' matches against. */
  analysis?: string | null;
  value?: IAutomationValueSource | null;
  field?: string | null;
  /** A draft name created earlier in the iteration; omitted for the subject. */
  target?: string | null;
  reason?: string | null;
  filter?: number | null;
  into?: string | null;
  /** The source collection of a collection operation. */
  from?: string | null;
  /** The second operand of union/except/intersect. */
  with?: string | null;
  /** '$item' or a draft name. */
  item?: string | null;
  by?: string | null;
  direction?: string | null;
  where?: IAutomationCondition | null;
  count?: number | null;
  fields?: string[] | null;
  max?: number | null;
  truncate?: Record<string, number> | null;
  against?: string | null;
  mode?: string | null;
  batchSize?: number | null;
  maxComparisons?: number | null;
  /** dedupe: persist confirmed duplicates as content_link records and skip linked items. */
  remember?: boolean | null;
  onDuplicate?: string | null;
  prompt?: IAutomationPrompt | null;
  objective?: string | null;
  take?: number | null;
  /** Select Top Scored: keep every item scoring at or above this value. */
  minScore?: number | null;
  contentAction?: number | null;
  report?: number | null;
  notification?: number | null;
  using?: string | null;
  as?: string | null;
  copyFrom?: string | null;
  copyFields?: string[] | null;
  set?: Record<string, IAutomationValueSource> | null;
  index?: boolean | null;
  llmId?: number | null;
}

/** One action type's descriptor from the catalog; the editor renders action forms from these. */
export interface IAutomationActionDescriptor {
  type: string;
  label: string;
  /** How the action works and what each field does; shown under the Configuration label. */
  description?: string | null;
  category: string;
  requiresSubject: boolean;
  requiresPersistedId: boolean;
  usesLLM: boolean;
  phases: string[];
  fields: IAutomationFieldSpec[];
}

export interface IAutomationFieldSpec {
  name: string;
  kind: string;
  required: boolean;
  help?: string | null;
}

export interface IAutomationValidationError {
  path: string;
  message: string;
  severity: 'error' | 'warning';
}

/** One entry of a run's decision log. */
export interface IAutomationRunLogModel {
  id: number;
  runId: number;
  stepName: string;
  actionName?: string | null;
  actionType?: string | null;
  analysisName?: string | null;
  contentId?: number | null;
  attempt: number;
  isLLM: boolean;
  variant?: string | null;
  prompt?: string | null;
  response?: string | null;
  promptTokens?: number | null;
  completionTokens?: number | null;
  durationMs: number;
  outcome: string;
  detail?: string | null;
  createdOn: string;
}

export interface IAutomationRunLogPage {
  items: IAutomationRunLogModel[];
  page: number;
  qty: number;
  total: number;
}

export interface IAutomationRunLogFilter {
  step?: string;
  action?: string;
  outcome?: string;
  contentId?: number;
  search?: string;
  page?: number;
  qty?: number;
  /** 'asc' (default) or 'desc' by execution order. */
  direction?: 'asc' | 'desc';
}

export interface IAutomationExplainRequestModel {
  question: string;
  messages?: { role: string; content: string }[];
}

export interface IAutomationExplainResultModel {
  logId: number;
  answer: string;
  /** A proposed prompt revision extracted from the answer; applied only by an explicit save. */
  suggestedPrompt?: string | null;
  messages: { role: string; content: string }[];
}

/** The run summary persisted on the run (run.summary JSON with engineVersion 2). */
export interface IAutomationRunSummaryModel {
  engineVersion: number;
  isDryRun: boolean;
  isComparison: boolean;
  variantA?: IAutomationVariantSummaryModel | null;
  variantB?: IAutomationVariantSummaryModel | null;
  differences: { contentRef: string; onlyA: string[]; onlyB: string[] }[];
}

/** One item and the score a Score Content action recorded for it. */
export interface IAutomationScoredItemModel {
  contentRef: string;
  score: number;
  headline?: string | null;
  step?: string | null;
}

/** Every score recorded under one objective, with the distribution of those scores. */
export interface IAutomationScoreObjectiveModel {
  objective: string;
  steps: string[];
  items: IAutomationScoredItemModel[];
  /** Score → how many items carry it (JSON object keys are the scores). */
  distribution: Record<string, number>;
  /** Items whose value was not an integer and so were never scored. */
  unscored: number;
}

/** One Select Top Scored action's outcome: the ranking rule, what it kept, what it chose from. */
export interface IAutomationSelectionModel {
  objective: string;
  step?: string | null;
  action?: string | null;
  /** The ranking rule applied — no LLM is involved. */
  sortedBy: string;
  /** What was kept, in words ('the top 10', 'every item scoring 7 or higher'). Absent on runs
   *  recorded before the score threshold existed. */
  rule?: string | null;
  /** The count cap, or null when only a score threshold applied. */
  take?: number | null;
  /** The score threshold, or null when a fixed count was taken. */
  minScore?: number | null;
  candidates: number;
  /** How many candidates met the threshold before the count cap. Absent on older runs. */
  qualified?: number | null;
  into?: string | null;
  contentAction?: string | null;
  selected: IAutomationScoredItemModel[];
  distribution: Record<string, number>;
  /** Ranked keys that no longer resolved to an item and were dropped. */
  unresolved: string[];
}

/** One item a save action wrote, naming the fields the write carried. */
export interface IAutomationSaveModel {
  contentRef: string;
  step?: string | null;
  action?: string | null;
  collection?: string | null;
  headline?: string | null;
  fields: string[];
  /** 'saved', 'created', 'would-save' (dry run) or 'failed'. */
  outcome: string;
  indexed: boolean;
  error?: string | null;
}

export interface IAutomationVariantSummaryModel {
  steps: {
    name: string;
    phase: string;
    items: number;
    executions: number;
    skipped: number;
    excluded: number;
    aborted: number;
    failures: number;
    llmCalls: number;
    durationMs: number;
    notes?: string | null;
  }[];
  changes: {
    type: string;
    contentRef: string;
    field?: string | null;
    value?: string | null;
    step?: string | null;
  }[];
  collections: Record<string, number>;
  excluded: { contentRef: string; reason: string; step?: string | null }[];
  draftIds: Record<string, number>;
  /** Score Content results by objective. Absent on runs recorded before scoring was summarized. */
  scores?: IAutomationScoreObjectiveModel[] | null;
  /** Select Top Scored results. Absent on runs recorded before selections were summarized. */
  selections?: IAutomationSelectionModel[] | null;
  /** Save Collection / Save Content Now results. Absent on runs recorded before saves were summarized. */
  saves?: IAutomationSaveModel[] | null;
  llmCalls: number;
  promptTokens: number;
  completionTokens: number;
  durationMs: number;
  flushFailures: string[];
}
