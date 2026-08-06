import { type IAutomationRuleActionModel } from '../interfaces';
import { type ActionModalMode } from './ActionModalMode';

export interface IActionModalState {
  mode: ActionModalMode;
  stepIndex: number;
  actionIndex?: number;
  action: IAutomationRuleActionModel;
}
