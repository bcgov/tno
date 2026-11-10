import { getSortableOptions, OptionItem, type INotificationTemplateModel } from 'tno-core';

export const getNotificationTemplateOptions = (
  templates: INotificationTemplateModel[],
  currentId: number,
) => {
  return getSortableOptions(templates, currentId, [new OptionItem('[New Template]', 0)]);
};
