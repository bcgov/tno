import { IReportModel } from '@/shared';

export const isAutoSend = (report: IReportModel) => {
  if (report.events === undefined) return false;
  return (
    report.isEnabled &&
    report.events &&
    report.events.some(
      (schedule) =>
        schedule.isEnabled &&
        schedule.settings.autoSend &&
        !!schedule.startAt &&
        !!schedule.runOnWeekDays,
    )
  );
};
