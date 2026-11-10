import { IOptionItem, OptionItem } from 'tno-core';

enum DaysOfWeek {
  Monday = 'Monday',
  Tuesday = 'Tuesday',
  Wednesday = 'Wednesday',
  Thursday = 'Thursday',
  Friday = 'Friday',
  Saturday = 'Saturday',
  Sunday = 'Sunday',
}

export const daysOfWeek: IOptionItem[] = Object.keys(DaysOfWeek).map(
  (key) => new OptionItem(key, DaysOfWeek[key as keyof typeof DaysOfWeek]),
);
