import { type IAutomationRuleActionModel } from '../interfaces';
import { getActionSettingsGroup } from './getActionSettingsGroup';

/**
 * Merge values into one action-type's configuration group within the action's settings JSON,
 * leaving every other group untouched (an action can carry more than one).
 */
export const setActionSettingsGroup = (
  action: IAutomationRuleActionModel,
  group: string,
  values: Record<string, any>,
): IAutomationRuleActionModel => ({
  ...action,
  settings: {
    ...(action.settings ?? {}),
    [group]: { ...getActionSettingsGroup(action, group), ...values },
  },
});
