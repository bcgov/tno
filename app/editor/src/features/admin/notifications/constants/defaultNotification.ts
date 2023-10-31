import { INotificationModel, NotificationTypeName, ResendOptionName } from 'tno-core';

import { defaultNotificationTemplate } from './defaultNotificationTemplate';

export const defaultNotification: INotificationModel = {
  id: 0,
  name: '',
  description: '',
  templateId: 0,
  template: { ...defaultNotificationTemplate },
  settings: {
    searchUnpublished: false,
    size: 0,
  },
  query: {},
  isEnabled: false,
  isPublic: false,
  sortOrder: 0,
  subscribers: [],
  notificationType: NotificationTypeName.Email,
  alertOnIndex: false,
  resend: ResendOptionName.Never,
};
