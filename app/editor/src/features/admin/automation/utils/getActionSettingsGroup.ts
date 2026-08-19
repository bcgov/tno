import { type IAutomationRuleActionModel } from '../interfaces';

/**
 * Read one action-type's configuration group out of the action's settings JSON
 * (e.g. 'collection' for fetch-content, 'deduplicate' for deduplicate). Returns an empty object
 * when the action has no settings yet, so callers can read straight through to their defaults.
 */
export const getActionSettingsGroup = (
  action: IAutomationRuleActionModel | undefined,
  group: string,
): Record<string, any> => {
  const value = action?.settings?.[group];
  return value && typeof value === 'object' && !Array.isArray(value) ? value : {};
};
