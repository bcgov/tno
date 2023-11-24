import { IAuditColumnsModel } from '.';

export interface IScheduleModel extends IAuditColumnsModel {
  id: number;
  name: string;
  description: string;
  isEnabled: boolean;
  delayMS: number;
  runOn?: Date;
  startAt?: string;
  stopAt?: string;
  runOnlyOnce: boolean;
  repeat: boolean;
  runOnWeekDays: string;
  runOnMonths: string;
  dayOfMonth: number;
  requestedById?: number;
}
