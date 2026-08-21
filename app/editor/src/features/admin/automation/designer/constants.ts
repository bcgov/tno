import { type IOptionItem } from 'tno-core';

import { createOption } from '../utils';
import {
  type IAutomationAction,
  type IAutomationDefinition,
  type IAutomationPromptEntry,
  type IAutomationStep,
} from './interfaces';

export const PHASES = ['init', 'process', 'complete'] as const;

export const phaseOptions: IOptionItem[] = [
  createOption('init', 'init'),
  createOption('process', 'process'),
  createOption('complete', 'complete'),
];

/** Built-in engine prompts a profile can override by creating a library entry with the
 * reserved name. The text mirrors the engine default (AutomationEngine) so editing starts from it. */
export const DEFAULT_PROMPTS: Record<
  string,
  { description: string; text: string; action: string }
> = {
  'default-dedupe': {
    action: 'dedupe',
    description:
      'What Detect Duplicate sends when no prompt is selected. Edit to customize; delete to restore the built-in.',
    text: '<p>Compare the CURRENT story to each CANDIDATE story. Two stories are duplicates when they have the same (or a trivially reworded) headline, the same story text (the summary, or the body when there is no summary), and the same published date. If a candidate is a duplicate of the current story respond with "[DUPLICATE:{value}]" where {value} is the contentId of that candidate. If none are duplicates respond with nothing.</p><p>## Current Story</p><p>{content}</p><p>## Candidates</p><p>{candidates}</p>',
  },
};

/** Tokens for the compared story in Detect Duplicate prompts. Field tokens resolve per
 * candidate in iterate mode; batch prompts use {candidates} (the full list). */
export const CANDIDATE_TOKENS: { token: string; hint: string }[] = [
  { token: '{candidate.publishedOn}', hint: "The candidate's published date" },
  { token: '{candidate.headline}', hint: "The candidate's headline" },
  { token: '{candidate.byline}', hint: "The candidate's byline" },
  { token: '{candidate.source}', hint: "The candidate's source" },
  { token: '{candidate.summary}', hint: "The candidate's summary" },
  { token: '{candidate.body}', hint: "The candidate's body" },
  { token: '{candidate.story}', hint: 'Summary, or body when there is no summary' },
  { token: '{candidate.contentId}', hint: "The candidate's id (for [DUPLICATE:{value}])" },
  { token: '{candidates}', hint: 'The whole candidate list as JSON (batch mode)' },
];

export const stepSourceOptions: IOptionItem[] = [
  createOption('collection', 'collection'),
  createOption('filter', 'filter'),
];

export const conditionOpOptions: IOptionItem[] = [
  createOption('exists', 'exists'),
  createOption('is empty', 'isEmpty'),
  createOption('equals', 'equals'),
  createOption('not equals', 'notEquals'),
  createOption('in list', 'in'),
  createOption('not in list', 'notIn'),
  createOption('contains', 'contains'),
  createOption('starts with', 'startsWith'),
  createOption('matches regex', 'matches'),
  createOption('length less than', 'lengthLessThan'),
  createOption('length greater than', 'lengthGreaterThan'),
  createOption('greater than', 'greaterThan'),
  createOption('less than', 'lessThan'),
  createOption('has tag', 'hasTag'),
  createOption('has action', 'hasAction'),
  createOption('status is', 'statusIs'),
];

/** Operators whose value is a list (rendered/parsed as comma-separated text). */
export const LIST_OPS = ['in', 'notIn'];

/** Operators that take no value. */
export const VALUELESS_OPS = ['exists', 'isEmpty'];

export const runLogOutcomeOptions: IOptionItem[] = [
  createOption('Any outcome', ''),
  createOption('Executed', 'executed'),
  createOption('Confirmed', 'confirmed'),
  createOption('No match', 'not-confirmed'),
  createOption('Condition met', 'condition-passed'),
  createOption('Condition not met', 'condition-failed'),
  createOption('Skipped', 'skipped'),
  createOption('Excluded', 'excluded'),
  createOption('Aborted', 'aborted'),
  createOption('Failed', 'failed'),
  createOption('Saved', 'flushed'),
  createOption('Explain', 'explain'),
  createOption('Info', 'info'),
];

/** Friendly display labels for stored outcome values: routine non-events ('condition-failed',
 * 'not-confirmed') read as normal flow, not errors. Stored values are unchanged. */
export const outcomeLabel = (outcome: string): string => {
  switch (outcome) {
    case 'not-confirmed':
      return 'no match';
    case 'condition-passed':
      return 'condition met';
    case 'condition-failed':
      return 'condition not met';
    case 'flushed':
      return 'saved';
    default:
      return outcome;
  }
};

export const createDefaultStep = (
  phase: IAutomationStep['phase'] = 'process',
): IAutomationStep => ({
  name: '',
  phase,
  isEnabled: true,
  source: phase === 'process' ? { from: 'collection', include: [], exclude: [] } : undefined,
  analyses: [],
  actions: [],
});

export const createDefaultAction = (type = 'content.update'): IAutomationAction => ({
  type,
  isEnabled: true,
});

export const createDefaultDefinition = (): IAutomationDefinition => ({
  prompts: {},
  steps: [
    {
      ...createDefaultStep('init'),
      name: 'Initialize',
    },
    {
      ...createDefaultStep('process'),
      name: 'Process content',
    },
    {
      ...createDefaultStep('complete'),
      name: 'Complete',
    },
  ],
});

/** Serialize a definition for storage. Prompt entries without a description collapse back to the
 * compact bare-string form the engine also accepts. */
export const serializeDefinition = (definition: IAutomationDefinition): string =>
  JSON.stringify({
    ...definition,
    prompts: Object.fromEntries(
      Object.entries(definition.prompts).map(([name, entry]) => [
        name,
        entry.description ? entry : entry.text,
      ]),
    ),
  });

/** Normalize a prompt entry from either stored shape (bare string or object). */
export const toPromptEntry = (value: unknown): IAutomationPromptEntry => {
  if (typeof value === 'string') return { text: value };
  const entry = value as Partial<IAutomationPromptEntry> | null;
  return { text: entry?.text ?? '', description: entry?.description ?? undefined };
};

export const parseDefinition = (json?: string | null): IAutomationDefinition => {
  if (!json) return createDefaultDefinition();
  try {
    const parsed = JSON.parse(json) as Partial<IAutomationDefinition>;
    return {
      prompts: Object.fromEntries(
        Object.entries(parsed.prompts ?? {}).map(([name, value]) => [name, toPromptEntry(value)]),
      ),
      steps: (parsed.steps ?? []).map((step) => ({
        ...step,
        isEnabled: step.isEnabled ?? true,
        analyses: step.analyses ?? [],
        actions: step.actions ?? [],
        name: step.name ?? '',
        phase: (step.phase as IAutomationStep['phase']) ?? 'process',
      })),
    };
  } catch {
    return createDefaultDefinition();
  }
};

/** Width (in ch) that fits a select's widest option label plus its control chrome
 * (padding + indicators), so fields size to the values inside their dropdowns.
 * extraCh covers additional injected indicators (e.g. the filter field's pencil and +). */
export const fitSelectWidth = (
  labels: (string | number | undefined)[],
  placeholder = '',
  extraCh = 0,
  minCh = 12,
  maxCh = 40,
): string => {
  const longest = Math.max(placeholder.length, ...labels.map((label) => `${label ?? ''}`.length));
  return `${Math.min(maxCh, Math.max(minCh, longest + 8 + extraCh))}ch`;
};

/** All filter ids referenced inside a definition: step sources, include/exclude gates, and
 * action filters. Export bundles their definitions; import remaps them. */
export const collectFilterIds = (definition: IAutomationDefinition): number[] => {
  const ids = new Set<number>();
  definition.steps.forEach((step) => {
    if (step.source?.filter != null) ids.add(step.source.filter);
    (step.source?.include ?? []).forEach((id) => ids.add(id));
    (step.source?.exclude ?? []).forEach((id) => ids.add(id));
    step.actions.forEach((action) => {
      if (action.filter != null) ids.add(action.filter);
    });
  });
  return Array.from(ids);
};

/** All LLM ids referenced inside a definition (step/analysis/action overrides). */
export const collectLlmIds = (definition: IAutomationDefinition): number[] => {
  const ids = new Set<number>();
  definition.steps.forEach((step) => {
    if (step.llmId != null) ids.add(step.llmId);
    step.analyses.forEach((analysis) => {
      if (analysis.llmId != null) ids.add(analysis.llmId);
    });
    step.actions.forEach((action) => {
      if (action.llmId != null) ids.add(action.llmId);
    });
  });
  return Array.from(ids);
};

/** Rewrite every filter/LLM id inside a definition through the import id maps. */
export const remapDefinition = (
  definition: IAutomationDefinition,
  mapFilter: (id?: number) => number | undefined,
  mapLLM: (id?: number) => number | undefined,
): IAutomationDefinition => ({
  ...definition,
  steps: definition.steps.map((step) => ({
    ...step,
    llmId: step.llmId != null ? mapLLM(step.llmId) ?? step.llmId : step.llmId,
    source: step.source
      ? {
          ...step.source,
          filter:
            step.source.filter != null
              ? mapFilter(step.source.filter) ?? step.source.filter
              : step.source.filter,
          include: (step.source.include ?? []).map((id) => mapFilter(id) ?? id),
          exclude: (step.source.exclude ?? []).map((id) => mapFilter(id) ?? id),
        }
      : step.source,
    analyses: step.analyses.map((analysis) => ({
      ...analysis,
      llmId: analysis.llmId != null ? mapLLM(analysis.llmId) ?? analysis.llmId : analysis.llmId,
    })),
    actions: step.actions.map((action) => ({
      ...action,
      filter: action.filter != null ? mapFilter(action.filter) ?? action.filter : action.filter,
      llmId: action.llmId != null ? mapLLM(action.llmId) ?? action.llmId : action.llmId,
    })),
  })),
});

/** Collection names created anywhere in the definition, for pickers and hints. */
export const collectCollectionNames = (definition: IAutomationDefinition): string[] => {
  const names = new Set<string>();
  definition.steps.forEach((step) => {
    step.actions.forEach((action) => {
      if (action.into) names.add(action.into);
    });
    if (step.source?.collection) names.add(step.source.collection);
  });
  return Array.from(names).sort();
};

/** Insertable data tokens: replaced at prompt composition with consistent, readable values from
 * the run's lookup bundle. Hints render as hover titles (the token's rendered form). */
export const LOOKUP_TOKENS: { token: string; hint: string }[] = [
  { token: '{lookup:tags}', hint: 'Enabled tags: CODE | Name — Description, one per line' },
  { token: '{lookup:contributors}', hint: 'Enabled contributors/columnists by name' },
  { token: '{lookup:sources}', hint: 'Enabled sources: CODE | Name — Description' },
  { token: '{lookup:mediaTypes}', hint: 'Enabled media types by name' },
  { token: '{lookup:actions}', hint: 'Enabled content actions by name' },
  { token: '{lookup:topics}', hint: 'Enabled topics by name' },
];

/** Insertable content tokens: replaced with the item's working copy (deltas folded in). */
export const CONTENT_TOKENS: { token: string; hint: string }[] = [
  { token: '{content}', hint: "The item's full working copy as JSON (changes included)" },
  { token: '{content.status}', hint: 'e.g. Published (reflects a pending publish/unpublish)' },
  { token: '{content.contentType}', hint: 'e.g. PrintContent, AudioVideo' },
  { token: '{content.headline}', hint: 'The headline' },
  { token: '{content.byline}', hint: 'The byline' },
  { token: '{content.body}', hint: 'The full story body (only capped by an explicit truncate)' },
  { token: '{content.summary}', hint: 'The summary' },
  { token: '{content.story}', hint: 'Summary, or body when there is no summary' },
  { token: '{content.source.name}', hint: 'e.g. Vancouver Sun' },
  { token: '{content.source.code}', hint: 'e.g. SUN' },
  { token: '{content.otherSource}', hint: 'The source code text on the item' },
  { token: '{content.mediaType.name}', hint: 'e.g. Newspaper' },
  { token: '{content.series.name}', hint: 'The show/program name, when any' },
  { token: '{content.contributor.name}', hint: 'The columnist (reflects a pending selection)' },
  { token: '{content.edition}', hint: 'The paper edition' },
  { token: '{content.section}', hint: 'e.g. A, Business' },
  { token: '{content.page}', hint: 'e.g. A1' },
  { token: '{content.publishedOn}', hint: 'The published date, yyyy-MM-dd' },
  {
    token: '{content.tags}',
    hint: 'JSON array of tag codes, e.g. ["hth","edu"] (includes pending adds)',
  },
  {
    token: '{content.sentiment}',
    hint: 'The default tone pool value, -5 to 5 (reflects a pending value)',
  },
  { token: '{content.actions}', hint: 'JSON array of applied content action names' },
  { token: '{content.labels}', hint: 'Comma-separated label values' },
  { token: '{content.topics}', hint: 'JSON array of {"name","score"} topic objects' },
];

/** The working-copy property fields conditions can test, derived from the content token list so
 * the dropdown and the prompt tokens stay one surface. */
export const contentTokenFieldOptions: IOptionItem[] = CONTENT_TOKENS.filter(({ token }) =>
  token.startsWith('{content.'),
).map(({ token }) => {
  // Strip the '{content.' prefix and the closing '}' ('{content.headline}' -> 'headline').
  const field = token.slice('{content.'.length, -1);
  return createOption(field, field);
});

/** Digest field options for fields pickers (search 'fields', create 'copyFields'): the prompt
 * token fields plus the identity ids a created row needs but which are noise as prompt tokens. */
export const copyFieldOptions: IOptionItem[] = [
  ...contentTokenFieldOptions,
  createOption('sourceId', 'sourceId'),
  createOption('licenseId', 'licenseId'),
  createOption('mediaTypeId', 'mediaTypeId'),
  createOption('uid', 'uid'),
  createOption('publishedOnUtc', 'publishedOnUtc'),
];

/** Outcomes that indicate a decision rather than an LLM exchange. */
export const outcomeBadgeClass = (outcome: string): string => {
  switch (outcome) {
    case 'executed':
    case 'confirmed':
    case 'condition-passed':
    case 'flushed':
      return 'automation-badge automation-badge-success';
    case 'failed':
      return 'automation-badge automation-badge-danger';
    case 'excluded':
    case 'aborted':
      return 'automation-badge automation-badge-warning';
    default:
      return 'automation-badge';
  }
};
