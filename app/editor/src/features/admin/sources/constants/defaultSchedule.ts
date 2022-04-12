import {
  IScheduleModel,
  ScheduleMonthName,
  ScheduleType,
  ScheduleWeekDayName,
} from 'hooks/api-editor';

export const defaultSchedule: IScheduleModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  scheduleType: ScheduleType.Repeating,
  delayMS: 0,
  repeat: 0,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
};
