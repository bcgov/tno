import { IFolderModel, ScheduleMonthName, ScheduleWeekDayName } from 'tno-core';

import { IFolderForm } from '../interfaces';

export const toModel = (form: IFolderForm): IFolderModel => {
  return {
    ...form,
    schedule: form.schedule
      ? {
          ...form.schedule,
          runOnWeekDays:
            form.schedule.runOnWeekDays === ''
              ? ScheduleWeekDayName.NA
              : form.schedule.runOnWeekDays,
          runOnMonths:
            form.schedule.runOnMonths === '' ? ScheduleMonthName.NA : form.schedule.runOnMonths,
        }
      : undefined,
    content: form.content.map((item) => ({ ...item, contentId: item.content.id })),
  };
};
