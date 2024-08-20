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
  startDate: Date,
  startTime: string,
  seriesId?: number,
  sourceId?: number,
) => {
  const startTimeParts = startTime.split(':');
  const seriesIds: number[] = seriesId ? [seriesId!] : [];
  const sourceIds: number[] = sourceId ? [sourceId!] : [];
  if (startTime) {
    startDate.setHours(
      Number(startTimeParts[0]),
      Number(startTimeParts[1]),
      Number(startTimeParts[2]),
    );
  }

  return generateQuery({
    searchUnpublished: false,
    size: 0,
    seriesIds,
    startDate: startDate.toISOString(),
    sourceIds,
    sort: [{ publishedOn: 'asc' }],
  });
};
