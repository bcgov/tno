import { IAutomationRuleActionModel } from './IAutomationRuleActionModel';

export interface IAutomationRuleModel {
  id: number;
  name: string;
  description: string;
  prompt: string;
  target: 'content' | 'start' | 'end';
  filterId?: number;
  applyToAutomationFilter: boolean;
  priority: number;
  isEnabled: boolean;
  actions: IAutomationRuleActionModel[];
}

export type IAutomationStepModel = IAutomationRuleModel;
