import { IAlertModel } from 'tno-core';

export const defaultAlert: IAlertModel = {
  id: 0,
  name: '',
  description: 'Alert to be displayed on the subscriber and editor application login.',
  isEnabled: true,
  sortOrder: 0,
  message: '',
};
