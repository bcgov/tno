import { WeekDay, WeekDayName } from 'hooks/api-editor';

export const weekDayName = (value: WeekDay | WeekDay[]) => {
  var values = Array.isArray(value) ? value : [value];

  if (
    values.includes(WeekDay.Sunday) &&
    values.includes(WeekDay.Monday) &&
    values.includes(WeekDay.Tuesday) &&
    values.includes(WeekDay.Wednesday) &&
    values.includes(WeekDay.Thursday) &&
    values.includes(WeekDay.Friday) &&
    values.includes(WeekDay.Saturday)
  )
    return 'All';

  if (
    values.includes(WeekDay.Sunday) &&
    values.includes(WeekDay.Saturday) &&
    ((values.includes(WeekDay.NA) && values.length === 3) || values.length === 2)
  )
    return 'Weekends';

  if (
    !values.includes(WeekDay.Sunday) &&
    values.includes(WeekDay.Monday) &&
    values.includes(WeekDay.Tuesday) &&
    values.includes(WeekDay.Wednesday) &&
    values.includes(WeekDay.Thursday) &&
    values.includes(WeekDay.Friday) &&
    !values.includes(WeekDay.Saturday)
  )
    return 'Weekdays';

  const result = values
    .filter((v) => v !== WeekDay.NA)
    .map((v) => {
      switch (v) {
        case WeekDay.Sunday:
          return WeekDayName.Sunday;
        case WeekDay.Monday:
          return WeekDayName.Monday;
        case WeekDay.Tuesday:
          return WeekDayName.Tuesday;
        case WeekDay.Wednesday:
          return WeekDayName.Wednesday;
        case WeekDay.Thursday:
          return WeekDayName.Thursday;
        case WeekDay.Friday:
          return WeekDayName.Friday;
        case WeekDay.Saturday:
          return WeekDayName.Saturday;
        default:
          return WeekDayName.NA;
      }
    });

  return result.join(', ');
};
