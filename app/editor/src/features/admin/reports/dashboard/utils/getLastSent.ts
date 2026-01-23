import { formatDashboardDate, type IReportModel } from 'tno-core';

export const getLastSent = (report: IReportModel) => {
  const sentOn = report.instances.find((i) => i.sentOn !== undefined)?.sentOn;
  if (report.instances.length === 0 || !sentOn) return 'Never';
  return formatDashboardDate(sentOn, true);
};
