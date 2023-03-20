import { ScheduleWeekDayName } from 'tno-core';

export const weekDayNameAbbrev = (
  value: string | string[] | ScheduleWeekDayName | ScheduleWeekDayName[],
) => {
  var values = Array.isArray(value)
    ? value
    : value
        .split(',')
        .map((v) => v.trim())
        .filter((v) => v !== ScheduleWeekDayName.NA);

  if (
    values.includes(ScheduleWeekDayName.Sunday) &&
    values.includes(ScheduleWeekDayName.Monday) &&
    values.includes(ScheduleWeekDayName.Tuesday) &&
    values.includes(ScheduleWeekDayName.Wednesday) &&
    values.includes(ScheduleWeekDayName.Thursday) &&
    values.includes(ScheduleWeekDayName.Friday) &&
    values.includes(ScheduleWeekDayName.Saturday)
  )
    return 'All';

  if (
    values.includes(ScheduleWeekDayName.Sunday) &&
    values.includes(ScheduleWeekDayName.Saturday) &&
    ((values.includes(ScheduleWeekDayName.NA) && values.length === 3) || values.length === 2)
  )
    return 'Weekends';

  if (
    !values.includes(ScheduleWeekDayName.Sunday) &&
    values.includes(ScheduleWeekDayName.Monday) &&
    values.includes(ScheduleWeekDayName.Tuesday) &&
    values.includes(ScheduleWeekDayName.Wednesday) &&
    values.includes(ScheduleWeekDayName.Thursday) &&
    values.includes(ScheduleWeekDayName.Friday) &&
    !values.includes(ScheduleWeekDayName.Saturday)
  )
    return 'Weekdays';

  return values.join(', ');
};
