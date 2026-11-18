import { type INotificationTemplateModel } from '@/shared';

export const defaultNotificationTemplate: INotificationTemplateModel = {
  id: 0,
  name: '',
  description: '',
  subject: '',
  body: '',
  isEnabled: true,
  isPublic: false,
  sortOrder: 0,
  settings: {},
};
