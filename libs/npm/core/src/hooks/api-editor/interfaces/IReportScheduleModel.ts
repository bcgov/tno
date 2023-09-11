import { IAuditColumnsModel } from '.';

export interface IReportScheduleModel extends IAuditColumnsModel {
  id: number;
  name: string;
  description: string;
  isEnabled: boolean;
  settings: any;
  requestSentOn?: Date | string;
  lastRanOn?: Date | string;
  scheduleId: number;
  scheduleVersion?: number;
  delayMS: number;
  runOn?: Date | string;
  startAt?: string;
  stopAt?: string;
  runOnlyOne: boolean;
  repeat: boolean;
  runOnWeekDays: string;
  runOnMonths: string;
  dayOfMonth: number;
  requestedById?: number;
}
