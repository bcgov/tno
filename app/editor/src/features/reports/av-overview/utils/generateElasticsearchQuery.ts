import moment from 'moment';
import { generateQuery } from 'tno-core';

/**
 * Generate an elasticsearch query.
 * @param startDate
 * @param startTime
 * @param seriesId
 * @param sourceId
 * @returns string
 */
export const generateElasticsearchQuery = (
  startDate: string | Date,
  startTime: string,
  seriesId?: number,
  sourceId?: number,
) => {
  const seriesIds: number[] = seriesId ? [seriesId!] : [];
  const sourceIds: number[] = sourceId ? [sourceId!] : [];

  let startDateValue = moment.utc(startDate);
  if (startTime) {
    const startTimeParts = startTime.split(':');
    startDateValue = startDateValue
      .add(startTimeParts[0], 'hours')
      .add(startTimeParts[1], 'minutes')
      .add(startTimeParts[2], 'seconds');
  }
  const endDate = moment(startDateValue).add(24, 'hours');

  return generateQuery({
    searchUnpublished: false,
    size: 0,
    seriesIds,
    startDate: startDateValue.toISOString(),
    endDate: endDate.toISOString(),
    sourceIds,
    sort: [{ publishedOn: 'asc' }],
  });
};
