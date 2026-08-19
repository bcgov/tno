import { type IOptionItem } from 'tno-core';

import { createOption } from '../utils';
import { type IV2Action, type IV2Definition, type IV2Step } from './interfaces';

export const V2_PHASES = ['init', 'process', 'complete'] as const;

export const v2PhaseOptions: IOptionItem[] = [
  createOption('Initialize (runs once, first)', 'init'),
  createOption('Process (runs per content item)', 'process'),
  createOption('Complete (runs once, last)', 'complete'),
];

export const v2SaveModeOptions: IOptionItem[] = [
  createOption('End of run (one write per item)', 'end-of-run'),
  createOption('End of step (write after each step)', 'end-of-step'),
];

export const v2StepSaveModeOptions: IOptionItem[] = [
  createOption('Inherit from profile', ''),
  ...v2SaveModeOptions,
];

export const v2SourceOptions: IOptionItem[] = [
  createOption('Profile filter results', 'profile'),
  createOption('Run a filter', 'filter'),
  createOption('A collection from the run', 'collection'),
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
  source: phase === 'process' ? { from: 'profile', include: [], exclude: [] } : undefined,
  analyses: [],
  actions: [],
});

export const createDefaultV2Action = (type = 'content.update'): IV2Action => ({
  type,
  isEnabled: true,
});

export const createDefaultV2Definition = (): IV2Definition => ({
  prompts: {},
  saveMode: 'end-of-run',
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

/** Serialize a definition for storage, dropping empty optional members so the stored document
 * stays close to what the engine reads. */
export const serializeV2Definition = (definition: IV2Definition): string =>
  JSON.stringify(definition);

export const parseV2Definition = (json?: string | null): IV2Definition => {
  if (!json) return createDefaultV2Definition();
  try {
    const parsed = JSON.parse(json) as Partial<IV2Definition>;
    return {
      prompts: parsed.prompts ?? {},
      saveMode: parsed.saveMode ?? 'end-of-run',
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
