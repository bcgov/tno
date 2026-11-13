import { getSortableOptions, type INotificationModel } from '@/shared';

export const getNotificationOptions = (templates: INotificationModel[], currentId: number) => {
  return getSortableOptions(templates, currentId);
};
