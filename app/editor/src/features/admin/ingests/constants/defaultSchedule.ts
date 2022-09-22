import {
  IScheduleModel,
  ScheduleMonthName,
  ScheduleTypeName,
  ScheduleWeekDayName,
} from 'hooks/api-editor';

export const defaultSchedule = (
  scheduleType: ScheduleTypeName,
): Omit<IScheduleModel, 'delayMS'> & { delayMS: number | '' } => ({
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  scheduleType: scheduleType,
  delayMS: '',
  repeat: 0,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
});
