import { getSortableOptions, INotificationModel } from 'tno-core';

export const getNotificationOptions = (templates: INotificationModel[], currentId: number) => {
  return getSortableOptions(templates, currentId);
};
