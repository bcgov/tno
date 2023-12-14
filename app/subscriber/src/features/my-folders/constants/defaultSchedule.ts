import { IFolderScheduleModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

export const defaultSchedule: IFolderScheduleModel = {
  id: 0,
  scheduleId: 0,
  name: '',
  description: '',
  isEnabled: true,
  delayMS: 3000000,
  runOnlyOnce: false,
  repeat: false,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
  settings: {},
};
