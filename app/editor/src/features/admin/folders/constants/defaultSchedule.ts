import { IScheduleModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

export const defaultSchedule: IScheduleModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  delayMS: 3000000,
  runOnlyOnce: false,
  repeat: false,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
};
