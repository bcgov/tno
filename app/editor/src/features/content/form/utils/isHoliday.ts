import moment from 'moment';
import { IHolidayModel } from 'tno-core';

/**
 * Determines if the specified 'date' is a stat holiday.
 * @param date The date to check if it is a stat holiday.
 * @param holidays An array of holidays.
 * @returns True if the date is a stat holiday.
 */
export const isHoliday = (date: Date | string, holidays: IHolidayModel[]) => {
  const value = moment(date);
  const stat = holidays.find((h) => moment(h.date).isSame(value, 'date'));
  return !!stat;
};
