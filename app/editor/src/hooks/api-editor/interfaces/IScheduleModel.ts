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
  startAt?: Date;
  stopAt?: Date;
  repeat: number;
  runOnWeekDays: string;
  runOnMonths: string;
  dayOfMonth: number;
}
