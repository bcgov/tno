import { IFolderModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

import { IFolderForm } from '../interfaces';

export const toForm = (model: IFolderModel): IFolderForm => {
  return {
    ...model,
    events: model.events.map((e) => ({
      ...e,
      runOnWeekDays: e.runOnWeekDays === '' ? ScheduleWeekDayName.NA : e.runOnWeekDays,
      runOnMonths: e.runOnMonths === '' ? ScheduleMonthName.NA : e.runOnMonths,
    })),
    content: model.content.map((item) => ({ ...item, content: item.content!, selected: false })),
  };
};
