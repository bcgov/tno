import { generateQuery } from '@/shared';
import { dayjs } from '@/shared/utils/date';

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
  const seriesIds: number[] = seriesId ? [seriesId] : [];
  const sourceIds: number[] = sourceId ? [sourceId] : [];

  let startDateValue = dayjs.utc(startDate);
  if (startTime) {
    const startTimeParts = startTime.split(':');
    startDateValue = startDateValue
      .add(Number(startTimeParts[0]), 'hours')
      .add(Number(startTimeParts[1]), 'minutes')
      .add(Number(startTimeParts[2]), 'seconds');
  }
  const endDate = dayjs(startDateValue).add(24, 'hours');

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
