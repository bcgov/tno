import { IReportModel } from 'tno-core';

export const isAutoSendDisabled = (report: IReportModel) => {
  if (report.events === undefined) return false;
  return (
    !report.isEnabled ||
    (report.events &&
      report.events.every(
        (schedule) => !schedule.isEnabled || !schedule.startAt || !schedule.runOnWeekDays,
      ))
  );
};
