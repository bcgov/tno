import { IReportModel } from 'tno-core';

/**
 * Calculates the next time the report will be sent.
 * @param report Report model.
 * @returns The next send on date.
 */
export const calcNextSend = (report: IReportModel) => {
  const schedules = report.schedules.filter((s) => s.isEnabled && s.settings.autoSend);
  if (!schedules.length) return 'NA';

  // const now = moment(Date.now());
  // const dayOfWeek = now.day;
  // Determine which schedule runs next.
  // TODO: Need to figure this out still.
  return 'NOT WORKING';
  // return moment(report.updatedOn).format('yyyy-MM-DD hh:mm:ssA');
};
