import { getSortableOptions, OptionItem, type INotificationTemplateModel } from '@/shared';

export const getNotificationTemplateOptions = (
  templates: INotificationTemplateModel[],
  currentId: number,
) => {
  return getSortableOptions(templates, currentId, [new OptionItem('[New Template]', 0)]);
};
