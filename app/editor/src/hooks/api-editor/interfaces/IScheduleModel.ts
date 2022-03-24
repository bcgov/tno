import { ScheduleType } from '../constants';
import { IAuditColumnsModel } from '.';

export interface IScheduleModel extends IAuditColumnsModel {
  id: number;
  name: string;
  description: string;
  enabled: boolean;
  scheduleType: ScheduleType;
  delayMS: number;
  runOn?: Date;
  startAt?: Date;
  stopAt?: Date;
  repeat: number;
  runOnWeekDays: number[];
  runOnMonths: number[];
  dayOfMonth: number;
}
