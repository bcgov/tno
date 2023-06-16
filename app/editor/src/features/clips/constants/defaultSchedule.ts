import { IScheduleModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

export const defaultSchedule: IScheduleModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  delayMS: 30000,
  runOn: undefined,
  startAt: undefined,
  stopAt: undefined,
  runOnlyOnce: true,
  repeat: false,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
};
