export interface IAutomationRuleActionModel {
  prompt: string;
  actionType: string;
  maxCalls?: number | null;
  confirmationStatement: string;
  isEnabled: boolean;
}
