import { dayjs } from '@/shared/utils/date';

/**
 * Provides the cutoff time for the postedOn value.
 * @param hours Number of hours to go back.
 * @returns date and time offset.
 */
export const getCutoff = (hours: number = 12) => {
  const date = new Date();

  date.setHours(date.getHours() - hours);
  return dayjs(date).toISOString();
};
