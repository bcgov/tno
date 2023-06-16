import { IScheduleModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

export const defaultSchedule = (): Omit<IScheduleModel, 'delayMS'> & { delayMS: number | '' } => ({
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  delayMS: '',
  runOnlyOnce: false,
  repeat: false,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
});
