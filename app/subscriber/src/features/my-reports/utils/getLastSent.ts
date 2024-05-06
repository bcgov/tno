import { formatDate } from 'features/utils';
import { IReportModel } from 'tno-core';

/**
 * Finds the last sent on date.
 * @param report Report model.
 * @returns The last sent on date.
 */
export const getLastSent = (report: IReportModel) => {
  const sentOn = report.instances.find((i) => i.sentOn !== undefined)?.sentOn;
  if (!report.instances.length || !sentOn) return '';
  return formatDate(sentOn, true);
};
