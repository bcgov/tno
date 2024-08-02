import { IReportModel } from 'tno-core';

import { formatDate } from './formatDate';

export const getLastSent = (report: IReportModel) => {
  const sentOn = report.instances.find((i) => i.sentOn !== undefined)?.sentOn;
  if (!report.instances.length || !sentOn) return 'Never';
  return formatDate(sentOn, true);
};
