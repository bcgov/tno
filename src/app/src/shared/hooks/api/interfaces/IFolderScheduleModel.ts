import { IAuditColumnsModel } from '.';

export interface IFolderScheduleModel extends IAuditColumnsModel {
  id: number;
  name: string;
  description: string;
  isEnabled: boolean;
  settings: any;
  requestSentOn?: string;
  lastRanOn?: string;
  scheduleId: number;
  scheduleVersion?: number;
  delayMS: number;
  runOn?: string;
  startAt?: string;
  stopAt?: string;
  runOnlyOnce: boolean;
  repeat: boolean;
  runOnWeekDays: string;
  runOnMonths: string;
  dayOfMonth: number;
  requestedById?: number;
}
