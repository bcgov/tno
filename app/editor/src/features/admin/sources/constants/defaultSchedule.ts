import { IScheduleModel, ScheduleType } from 'hooks/api-editor';

export const defaultSchedule: IScheduleModel = {
  id: 0,
  name: '',
  description: '',
  enabled: true,
  scheduleType: ScheduleType.Repeating,
  delayMS: 0,
  repeat: 0,
  runOnWeekDays: [0],
  runOnMonths: [0],
  dayOfMonth: 0,
};
