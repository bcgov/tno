import { type IAutomationRuleActionModel } from '../interfaces';

export const cloneAction = (action: IAutomationRuleActionModel): IAutomationRuleActionModel => ({
  ...action,
});
