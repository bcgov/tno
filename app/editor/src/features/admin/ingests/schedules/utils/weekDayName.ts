import { ScheduleWeekDay, ScheduleWeekDayName } from 'hooks/api-editor';

export const weekDayName = (value: ScheduleWeekDay | ScheduleWeekDay[]) => {
  var values = Array.isArray(value) ? value : [value];

  if (
    values.includes(ScheduleWeekDay.Sunday) &&
    values.includes(ScheduleWeekDay.Monday) &&
    values.includes(ScheduleWeekDay.Tuesday) &&
    values.includes(ScheduleWeekDay.Wednesday) &&
    values.includes(ScheduleWeekDay.Thursday) &&
    values.includes(ScheduleWeekDay.Friday) &&
    values.includes(ScheduleWeekDay.Saturday)
  )
    return 'All';

  if (
    values.includes(ScheduleWeekDay.Sunday) &&
    values.includes(ScheduleWeekDay.Saturday) &&
    ((values.includes(ScheduleWeekDay.NA) && values.length === 3) || values.length === 2)
  )
    return 'Weekends';

  if (
    !values.includes(ScheduleWeekDay.Sunday) &&
    values.includes(ScheduleWeekDay.Monday) &&
    values.includes(ScheduleWeekDay.Tuesday) &&
    values.includes(ScheduleWeekDay.Wednesday) &&
    values.includes(ScheduleWeekDay.Thursday) &&
    values.includes(ScheduleWeekDay.Friday) &&
    !values.includes(ScheduleWeekDay.Saturday)
  )
    return 'Weekdays';

  const result = values
    .filter((v) => v !== ScheduleWeekDay.NA)
    .map((v) => {
      switch (v) {
        case ScheduleWeekDay.Sunday:
          return ScheduleWeekDayName.Sunday;
        case ScheduleWeekDay.Monday:
          return ScheduleWeekDayName.Monday;
        case ScheduleWeekDay.Tuesday:
          return ScheduleWeekDayName.Tuesday;
        case ScheduleWeekDay.Wednesday:
          return ScheduleWeekDayName.Wednesday;
        case ScheduleWeekDay.Thursday:
          return ScheduleWeekDayName.Thursday;
        case ScheduleWeekDay.Friday:
          return ScheduleWeekDayName.Friday;
        case ScheduleWeekDay.Saturday:
          return ScheduleWeekDayName.Saturday;
        default:
          return ScheduleWeekDayName.NA;
      }
    });

  return result.join(', ');
};
