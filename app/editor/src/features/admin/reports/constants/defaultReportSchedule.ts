import {
  type IReportModel,
  type IReportScheduleModel,
  ScheduleMonthName,
  ScheduleWeekDayName,
} from 'tno-core';

export const defaultReportSchedule = (
  label: string,
  report?: IReportModel,
): IReportScheduleModel => ({
  id: 0,
  name: generateScheduleName(label, report),
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

export const generateScheduleName = (label: string, report?: IReportModel) => {
  const maxNameLength = 100;
  const nameSuffix = `-${report?.ownerId ?? 0}-${label}`;
  const namePrefix = `${report?.name ?? 'Report'}`.substring(0, maxNameLength - nameSuffix.length);
  return `${namePrefix}${nameSuffix}`;
};
