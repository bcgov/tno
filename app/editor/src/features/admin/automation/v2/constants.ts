import { type IOptionItem } from 'tno-core';

import { createOption } from '../utils';
import {
  type IV2Action,
  type IV2Definition,
  type IV2PromptEntry,
  type IV2Step,
} from './interfaces';

export const V2_PHASES = ['init', 'process', 'complete'] as const;

export const v2PhaseOptions: IOptionItem[] = [
  createOption('init', 'init'),
  createOption('process', 'process'),
  createOption('complete', 'complete'),
];

export const v2SourceOptions: IOptionItem[] = [
  createOption('collection', 'collection'),
  createOption('filter', 'filter'),
];

export const v2ConditionOpOptions: IOptionItem[] = [
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
export const V2_LIST_OPS = ['in', 'notIn'];

/** Operators that take no value. */
export const V2_VALUELESS_OPS = ['exists', 'isEmpty'];

export const v2RunLogOutcomeOptions: IOptionItem[] = [
  createOption('Any outcome', ''),
  createOption('Executed', 'executed'),
  createOption('Confirmed', 'confirmed'),
  createOption('Not confirmed', 'not-confirmed'),
  createOption('Condition passed', 'condition-passed'),
  createOption('Condition failed', 'condition-failed'),
  createOption('Skipped', 'skipped'),
  createOption('Excluded', 'excluded'),
  createOption('Aborted', 'aborted'),
  createOption('Failed', 'failed'),
  createOption('Flushed', 'flushed'),
  createOption('Explain', 'explain'),
  createOption('Info', 'info'),
];

export const createDefaultV2Step = (phase: IV2Step['phase'] = 'process'): IV2Step => ({
  name: '',
  phase,
  isEnabled: true,
  source: phase === 'process' ? { from: 'collection', include: [], exclude: [] } : undefined,
  analyses: [],
  actions: [],
});

export const createDefaultV2Action = (type = 'content.update'): IV2Action => ({
  type,
  isEnabled: true,
});

export const createDefaultV2Definition = (): IV2Definition => ({
  prompts: {},
  steps: [
    {
      ...createDefaultV2Step('init'),
      name: 'Initialize',
    },
    {
      ...createDefaultV2Step('process'),
      name: 'Process content',
    },
    {
      ...createDefaultV2Step('complete'),
      name: 'Complete',
    },
  ],
});

/** Serialize a definition for storage. Prompt entries without a description collapse back to the
 * compact bare-string form the engine also accepts. */
export const serializeV2Definition = (definition: IV2Definition): string =>
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
export const toPromptEntry = (value: unknown): IV2PromptEntry => {
  if (typeof value === 'string') return { text: value };
  const entry = value as Partial<IV2PromptEntry> | null;
  return { text: entry?.text ?? '', description: entry?.description ?? undefined };
};

export const parseV2Definition = (json?: string | null): IV2Definition => {
  if (!json) return createDefaultV2Definition();
  try {
    const parsed = JSON.parse(json) as Partial<IV2Definition>;
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
        phase: (step.phase as IV2Step['phase']) ?? 'process',
      })),
    };
  } catch {
    return createDefaultV2Definition();
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

/** Collection names created anywhere in the definition, for pickers and hints. */
export const collectV2CollectionNames = (definition: IV2Definition): string[] => {
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
export const V2_LOOKUP_TOKENS: { token: string; hint: string }[] = [
  { token: '{lookup:tags}', hint: 'Enabled tags: CODE | Name — Description, one per line' },
  { token: '{lookup:contributors}', hint: 'Enabled contributors/columnists by name' },
  { token: '{lookup:sources}', hint: 'Enabled sources: CODE | Name — Description' },
  { token: '{lookup:mediaTypes}', hint: 'Enabled media types by name' },
  { token: '{lookup:actions}', hint: 'Enabled content actions by name' },
  { token: '{lookup:topics}', hint: 'Enabled topics by name' },
];

/** Insertable content tokens: replaced with the item's working copy (deltas folded in). */
export const V2_CONTENT_TOKENS: { token: string; hint: string }[] = [
  { token: '{content.status}', hint: 'e.g. Published (reflects a pending publish/unpublish)' },
  { token: '{content.contentType}', hint: 'e.g. PrintContent, AudioVideo' },
  { token: '{content.headline}', hint: 'The headline' },
  { token: '{content.byline}', hint: 'The byline' },
  { token: '{content.body}', hint: 'The story body (truncated per the digest settings)' },
  { token: '{content.summary}', hint: 'The summary' },
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
  { token: '{content.tags}', hint: 'Comma-separated tag codes (includes pending adds)' },
  { token: '{content.sentiment}', hint: '-5 to 5 (reflects a pending value)' },
  { token: '{content.actions}', hint: 'Comma-separated content action names' },
  { token: '{content.labels}', hint: 'Comma-separated label values' },
  { token: '{content.topics}', hint: 'Comma-separated topic names' },
];

/** The working-copy property fields conditions can test, derived from the content token list so
 * the dropdown and the prompt tokens stay one surface. */
export const v2ContentFieldOptions: IOptionItem[] = V2_CONTENT_TOKENS.map(({ token }) => {
  const field = token.replace('{content.', '').replace('}', '');
  return createOption(field, field);
});

/** Outcomes that indicate a decision rather than an LLM exchange. */
export const v2OutcomeBadgeClass = (outcome: string): string => {
  switch (outcome) {
    case 'executed':
    case 'confirmed':
    case 'condition-passed':
    case 'flushed':
      return 'v2-badge v2-badge-success';
    case 'failed':
      return 'v2-badge v2-badge-danger';
    case 'excluded':
    case 'aborted':
    case 'not-confirmed':
    case 'condition-failed':
      return 'v2-badge v2-badge-warning';
    default:
      return 'v2-badge';
  }
};
