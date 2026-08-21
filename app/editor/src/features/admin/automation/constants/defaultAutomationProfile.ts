import { type IAutomationProfileModel } from '../interfaces';

/** Content fields the content-update actions can target. */
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
 * The starter v2 definition for a new profile: one step per phase, mirroring
 * createDefaultV2Definition in ../v2 (kept inline as JSON so this constants module does not
 * import ../v2, which itself imports ../utils at module scope - a cycle).
 */
const defaultDefinition = JSON.stringify({
  prompts: {},
  steps: [
    { name: 'Initialize', phase: 'init', isEnabled: true, analyses: [], actions: [] },
    {
      name: 'Process content',
      phase: 'process',
      isEnabled: true,
      source: { from: 'collection', include: [], exclude: [] },
      analyses: [],
      actions: [],
    },
    { name: 'Complete', phase: 'complete', isEnabled: true, analyses: [], actions: [] },
  ],
});

export const defaultAutomationProfile: IAutomationProfileModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  schemaVersion: 2,
  definition: defaultDefinition,
  llmId: undefined,
  schedules: [],
};
