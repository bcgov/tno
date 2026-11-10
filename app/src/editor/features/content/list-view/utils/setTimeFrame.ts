import { dayjs } from '@/shared/utils/date';

/**
 * Function used to help filter content by time frame.
 * @param {number} timeFrame - Time frame passed down from the filter
 * @returns Time frame coupled to today, the last 24 hours, or the last 48 hours.
 */
export const setTimeFrame = (timeFrame: number) => {
  if (timeFrame === 0) return dayjs().startOf('day').toDate();
  else if (timeFrame === 1) return dayjs().add(-24, 'hours').toDate();
  else if (timeFrame === 2) return dayjs().add(-48, 'hours').toDate();
  return undefined;
};
