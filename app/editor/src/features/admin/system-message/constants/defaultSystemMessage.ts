import { ISystemMessageModel } from 'tno-core';

export const defaultSystemMessage: ISystemMessageModel = {
  id: 0,
  name: '',
  description: 'System message to be displayed on the subscriber and editor application login.',
  isEnabled: true,
  sortOrder: 0,
  message: '',
};
