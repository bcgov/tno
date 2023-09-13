import { IReportModel } from 'tno-core';

export const isAutoSendDisabled = (report: IReportModel) => {
  return (
    !report.isEnabled ||
    report.schedules.every(
      (schedule) => !schedule.isEnabled || !schedule.startAt || !schedule.runOnWeekDays,
    )
  );
};
