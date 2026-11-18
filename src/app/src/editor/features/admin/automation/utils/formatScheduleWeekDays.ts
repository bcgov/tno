import { scheduleWeekDayOptions } from './scheduleWeekDayOptions';

/** Format the selected week days for display. */
export const formatScheduleWeekDays = (days: number[]): string =>
  !days?.length
    ? 'Every day'
    : scheduleWeekDayOptions
        .filter((day) => days.includes(day.value))
        .map((day) => day.label)
        .join(', ');
