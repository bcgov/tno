import { IScheduleModel, ScheduleMonthName, ScheduleTypeName, ScheduleWeekDayName } from 'hooks';

export const defaultSchedule: IScheduleModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  scheduleType: ScheduleTypeName.Advanced,
  delayMS: 30000,
  runOn: undefined,
  startAt: undefined,
  stopAt: undefined,
  runOnlyOnce: true,
  repeat: 0,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
};
