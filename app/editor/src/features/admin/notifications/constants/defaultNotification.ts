import { INotificationModel, NotificationTypeName, ResendOptionName } from 'tno-core';

export const defaultNotification: INotificationModel = {
  id: 0,
  name: '',
  description: '',
  ownerId: 0,
  template: '',
  filter: {},
  settings: {},
  isEnabled: false,
  isPublic: false,
  sortOrder: 0,
  subscribers: [],
  notificationType: NotificationTypeName.Email,
  requireAlert: false,
  resend: ResendOptionName.Never,
};
