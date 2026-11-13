import { ListOptionName, type IMediaTypeModel } from '@/shared';

export const defaultMediaType: IMediaTypeModel = {
  id: 0,
  name: '',
  description: '',
  autoTranscribe: false,
  isEnabled: true,
  sortOrder: 0,
  settings: {},
  listOption: ListOptionName.Source,
};
