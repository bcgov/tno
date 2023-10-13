import {
  IReportModel,
  IReportScheduleModel,
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
  runOnlyOne: false,
  repeat: false,
  runOnWeekDays: ScheduleWeekDayName.NA,
  runOnMonths: ScheduleMonthName.NA,
  dayOfMonth: 0,
});

export const generateScheduleName = (label: string, report?: IReportModel) => {
  let maxNameLength = 100;
  let nameSuffix = `-${report?.ownerId ?? 0}-${label}`;
  let namePrefix = `${report?.name ?? 'Report'}`.substring(0, maxNameLength - nameSuffix.length);
  return `${namePrefix}${nameSuffix}`;
};
