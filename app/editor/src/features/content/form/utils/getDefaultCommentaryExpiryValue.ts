import moment from 'moment';
import { IHolidayModel } from 'tno-core';

import { isHoliday } from '.';

/**
 * Provides the default commentary expiry value in hours for the specified 'date'.
 * Weekdays = 3 hours
 * Weekends = 5 hours
 * Holidays = 5 hours
 * @param date The date that will determine the default commentary expiry value.
 * @param holidays An array of holidays.
 * @returns The hours before the content expires from the commentary section.
 */
export const getDefaultCommentaryExpiryValue = (date: Date | string, holidays: IHolidayModel[]) => {
  const value = moment(date);
  const weekDay = value.weekday();
  console.debug(weekDay);
  // TODO: There may be an issue if local time has Monday as the start of the week.
  return isHoliday(date, holidays) ? 5 : weekDay === 0 || weekDay === 6 ? 5 : 3;
};
