import { ScheduleTypeName } from '../constants';
import { IAuditColumnsModel } from '.';

export interface IScheduleModel extends IAuditColumnsModel {
  id: number;
  name: string;
  description: string;
  isEnabled: boolean;
  scheduleType: ScheduleTypeName;
  delayMS: number;
  runOn?: Date;
  startAt?: string;
  stopAt?: string;
  repeat: number;
  runOnWeekDays: string;
  runOnMonths: string;
  dayOfMonth: number;
}
