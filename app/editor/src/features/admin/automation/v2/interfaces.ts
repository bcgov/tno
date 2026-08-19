/** One prompt library entry. Stored as a bare string when it has no description (legacy form);
 * the parser normalizes both shapes to this object. */
export interface IV2PromptEntry {
  text: string;
  /** What the prompt is for, shown in the library table. */
  description?: string | null;
}

/** The v2 profile definition document, stored as JSON on the profile (schemaVersion >= 2). */
export interface IV2Definition {
  /** The prompt library: named entries shared via prompt refs. */
  prompts: Record<string, IV2PromptEntry>;
  /** When accumulated content changes flush: 'end-of-run' (default) or 'end-of-step'. */
  saveMode: string;
  steps: IV2Step[];
}

export interface IV2Step {
  name: string;
  description?: string;
  /** Lifecycle phase: 'init' (once, first), 'process' (per item), 'complete' (once, last). */
  phase: 'init' | 'process' | 'complete';
  isEnabled: boolean;
  /** Where a process step's content comes from; required for 'process', absent otherwise. */
  source?: IV2Source;
  /** Optional flush-mode override for this step. */
  saveMode?: string | null;
  llmId?: number | null;
  analyses: IV2Analysis[];
  actions: IV2Action[];
}

export interface IV2Source {
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

export interface IV2Analysis {
  name: string;
  prompt: IV2Prompt;
  /** Continue an earlier analysis as a conversation (the model sees the earlier exchange). */
  chain?: string | null;
  /** Result shape: key -> type spec ('string', 'string?', 'string[]', 'bool', 'int', 'int(a..b)'). */
  returns: Record<string, string>;
  /** Raw mode: keep the response as text; actions gate on it with confirmation statements. */
  raw?: boolean;
  llmId?: number | null;
}

export interface IV2Prompt {
  /** A prompt library entry name. */
  ref?: string | null;
  /** Inline prompt text (used alone). */
  text?: string | null;
  /** Text layered onto the referenced library entry. */
  override?: string | null;
}

/** A declarative gate: exactly one shape — a leaf (field/op/value), a combinator (all/any/not),
 * or an analysis-result gate (from: 'analysisName.key'). */
export interface IV2Condition {
  field?: string | null;
  op?: string | null;
  value?: unknown;
  all?: IV2Condition[];
  any?: IV2Condition[];
  not?: IV2Condition | null;
  from?: string | null;
}

/** Where an action's value comes from — a fixed source, never an expression. */
export interface IV2ValueSource {
  /** 'analysisName.key' or 'content.field'. */
  from?: string | null;
  literal?: unknown;
  /** Token template, e.g. 'DIGEST: {content.headline}'. */
  template?: string | null;
}

export interface IV2Action {
  type: string;
  name?: string | null;
  isEnabled: boolean;
  /** Property condition / analysis gate; evaluated before any prompt is sent. */
  when?: IV2Condition | null;
  /** Confirmation statement matched against a raw analysis response ({value} capture). */
  confirm?: string | null;
  /** The analysis whose response 'confirm' matches against. */
  analysis?: string | null;
  value?: IV2ValueSource | null;
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
  where?: IV2Condition | null;
  count?: number | null;
  fields?: string[] | null;
  max?: number | null;
  truncate?: Record<string, number> | null;
  against?: string | null;
  mode?: string | null;
  batchSize?: number | null;
  maxComparisons?: number | null;
  onDuplicate?: string | null;
  prompt?: IV2Prompt | null;
  objective?: string | null;
  take?: number | null;
  contentAction?: number | null;
  report?: number | null;
  notification?: number | null;
  using?: string | null;
  as?: string | null;
  copyFrom?: string | null;
  copyFields?: string[] | null;
  set?: Record<string, IV2ValueSource> | null;
  index?: boolean | null;
  llmId?: number | null;
}

/** One action type's descriptor from the catalog; the editor renders action forms from these. */
export interface IV2ActionDescriptor {
  type: string;
  label: string;
  category: string;
  requiresSubject: boolean;
  requiresPersistedId: boolean;
  usesLLM: boolean;
  phases: string[];
  fields: IV2FieldSpec[];
}

export interface IV2FieldSpec {
  name: string;
  kind: string;
  required: boolean;
  help?: string | null;
}

export interface IV2ValidationError {
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

export interface IV2MigrateResultModel {
  profile: { id: number; name: string };
  warnings: string[];
}

/** The v2 run summary persisted on the run (run.summary JSON with engineVersion 2). */
export interface IV2RunSummaryModel {
  engineVersion: number;
  isDryRun: boolean;
  isComparison: boolean;
  variantA?: IV2VariantSummaryModel | null;
  variantB?: IV2VariantSummaryModel | null;
  differences: { contentRef: string; onlyA: string[]; onlyB: string[] }[];
}

export interface IV2VariantSummaryModel {
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
  llmCalls: number;
  promptTokens: number;
  completionTokens: number;
  durationMs: number;
  flushFailures: string[];
}
