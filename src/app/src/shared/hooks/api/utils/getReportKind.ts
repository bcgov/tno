import { IReportModel, ReportKindName } from '../';

/**
 * Get the type of report based on the schedules.
 * @param report Report data.
 * @returns The type of report.
 */
export const getReportKind = (report: IReportModel) => {
  if (report.events.some((e) => e.isEnabled && !e.settings.autoSend)) return ReportKindName.Auto;
  else if (report.events.some((e) => e.isEnabled && e.settings.autoSend))
    return ReportKindName.AutoSend;
  return ReportKindName.Manual;
};
