import { IAutomationRuleModel } from './IAutomationRuleModel';

export interface IAutomationProfileModel {
  id: number;
  name: string;
  description: string;
  isEnabled: boolean;
  schemaVersion: number;
  filterId?: number;
  llmId?: number;
  timeZone: string;
  scheduleCron?: string;
  steps: IAutomationRuleModel[];
  rules?: IAutomationRuleModel[];
}
