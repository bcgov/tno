import { getSortableOptions, type INotificationModel } from 'tno-core';

export const getNotificationOptions = (templates: INotificationModel[], currentId: number) => {
  return getSortableOptions(templates, currentId);
};
