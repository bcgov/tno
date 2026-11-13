import { AutomationRunStatus } from './IAutomationRunStatus';

export interface IAutomationRunModel {
  id: number;
  profileId: number;
  status: AutomationRunStatus;
  trigger: string;
  note?: string;
  startedOn: string;
  completedOn?: string;
}
