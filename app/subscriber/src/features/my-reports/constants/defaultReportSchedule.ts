import {
  IReportModel,
  IReportScheduleModel,
  ScheduleMonthName,
  ScheduleWeekDayName,
} from 'tno-core';

import { getScheduleName } from '../utils';

export const defaultReportSchedule = (
  label: string,
  report?: IReportModel,
): IReportScheduleModel => ({
  id: 0,
  name: getScheduleName(label, report),
  description: report?.description ?? '',
  isEnabled: false,
  settings: {
    autoSend: false,
  },
  delayMS: 30000,
  scheduleId: 0,
  runOnlyOnce: false,
  repeat: false,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
});
