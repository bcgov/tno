import { IReportModel } from 'tno-core';

export const isAutoSend = (report: IReportModel) => {
  return (
    report.isEnabled &&
    report.events.some(
      (schedule) =>
        schedule.isEnabled &&
        schedule.settings.autoSend &&
        !!schedule.startAt &&
        !!schedule.runOnWeekDays,
    )
  );
};
