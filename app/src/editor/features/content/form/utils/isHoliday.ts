import { type IHolidayModel } from '@/shared';
import { dayjs } from '@/shared/utils/date';

/**
 * Determines if the specified 'date' is a stat holiday.
 * @param date The date to check if it is a stat holiday.
 * @param holidays An array of holidays.
 * @returns True if the date is a stat holiday.
 */
export const isHoliday = (date: Date | string, holidays: IHolidayModel[]) => {
  const value = dayjs(date);
  const stat = holidays.find((h) => dayjs(h.date).isSame(value, 'date'));
  return !!stat;
};
