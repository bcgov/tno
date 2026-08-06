import { type IAutomationRuleActionModel, type IAutomationStepModel } from '../interfaces';
import { cloneAction } from './cloneAction';

export const cloneStep = (step: IAutomationStepModel): IAutomationStepModel => ({
  ...step,
  actions: (
    step.actions ??
    (
      step as IAutomationStepModel & {
        Actions?: IAutomationRuleActionModel[];
      }
    ).Actions ??
    []
  ).map(cloneAction),
});
