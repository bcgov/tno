import { IReportModel } from 'tno-core';

export const setAutoSend = (report: IReportModel, checked: boolean) => {
  return {
    ...report,
    schedules: report.schedules.map((schedule) => ({
      ...schedule,
      settings: { ...schedule.settings, autoSend: checked },
    })),
  };
};
