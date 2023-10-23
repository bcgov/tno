import { IFolderModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

import { IFolderForm } from '../interfaces';

export const toModel = (form: IFolderForm): IFolderModel => {
  return {
    ...form,
    events: form.events.map((e) => ({
      ...e,
      runOnWeekDays: e.runOnWeekDays === '' ? ScheduleWeekDayName.NA : e.runOnWeekDays,
      runOnMonths: e.runOnMonths === '' ? ScheduleMonthName.NA : e.runOnMonths,
    })),
    content: form.content.map((item) => ({ ...item, contentId: item.content.id })),
  };
};
