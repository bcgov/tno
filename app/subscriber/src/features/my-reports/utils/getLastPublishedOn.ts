import moment from 'moment';
import { IReportModel } from 'tno-core';

/**
 * Finds the last published on date.
 * @param report Report model.
 * @returns The last published on date.
 */
export const getLastPublishedOn = (report: IReportModel) => {
  const publishedOn = report.instances.find((i) => i.publishedOn !== undefined)?.publishedOn;
  return !!publishedOn ? moment(publishedOn).format('yyyy-MM-DD hh:mm:ssA') : '';
};
