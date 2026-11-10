import { ScheduleWeekDayName } from '../constants';

/**
 * Adds the ScheduleWeekDay.NA value if there are no values selected.
 * Removes the ScheduleWeeDay.NA value if there are other values selected.
 * @param values Comma separated list of ScheduleWeekDay key values.
 * @returns A new comma separate list of ScheduleWeekDay key values.
 */
export const selectWeekDays = (values: string | any[]) => {
  if (typeof values === 'string') {
    const keys = values.split(',');
    if (values.length && keys.length) {
      if (keys.includes(ScheduleWeekDayName.NA.toString()) && keys.length > 1) {
        // Remove NA value.
        return keys.filter((v) => v !== ScheduleWeekDayName.NA.toString()).join(',');
      }

      return values;
    }
    // Add NA value if there are no values.
    return ScheduleWeekDayName.NA.toString();
  }

  if (values.length) {
    if (values.includes(ScheduleWeekDayName.NA) && values.length > 1)
      return values.filter((v) => v !== ScheduleWeekDayName.NA);

    return values;
  }
  return [ScheduleWeekDayName.NA];
};
