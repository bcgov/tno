import { formatDashboardDate, IReportModel } from 'tno-core';

export const getLastSent = (report: IReportModel) => {
  const sentOn = report.instances.find((i) => i.sentOn !== undefined)?.sentOn;
  if (!report.instances.length || !sentOn) return 'Never';
  return formatDashboardDate(sentOn, true);
};
