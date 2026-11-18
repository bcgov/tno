import { IReportModel } from '@/shared';

export const setAutoSend = (report: IReportModel, checked: boolean) => {
  return {
    ...report,
    events: report.events.map((schedule) => ({
      ...schedule,
      settings: { ...schedule.settings, autoSend: checked },
    })),
  };
};
