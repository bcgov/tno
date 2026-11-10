import { type ISystemMessageModel } from '@/shared';

export const defaultSystemMessage: ISystemMessageModel = {
  id: 0,
  name: '',
  description: '',
  message: '',
  isEnabled: true,
  sortOrder: 0,
};
