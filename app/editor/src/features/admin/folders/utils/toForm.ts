import { IFolderModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

import { IFolderForm } from '../interfaces';

export const toForm = (model: IFolderModel): IFolderForm => {
  return {
    ...model,
    schedule: model.schedule
      ? {
          ...model.schedule,
          runOnWeekDays:
            model.schedule.runOnWeekDays === ''
              ? ScheduleWeekDayName.NA
              : model.schedule.runOnWeekDays,
          runOnMonths:
            model.schedule.runOnMonths === '' ? ScheduleMonthName.NA : model.schedule.runOnMonths,
        }
      : undefined,
    content: model.content.map((item) => ({ ...item, content: item.content!, selected: false })),
  };
};
