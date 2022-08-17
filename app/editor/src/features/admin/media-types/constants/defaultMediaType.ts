import { IMediaTypeModel } from 'hooks';

export const defaultMediaType: IMediaTypeModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  autoTranscribe: false,
  disableTranscribe: false,
};
