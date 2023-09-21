import { IReportModel } from 'tno-core';

export const isAutoSend = (report: IReportModel) => {
  return (
    report.isEnabled &&
    report.schedules.some(
      (schedule) =>
        schedule.isEnabled &&
        schedule.settings.autoSend &&
        !!schedule.startAt &&
        !!schedule.runOnWeekDays,
    )
  );
};
