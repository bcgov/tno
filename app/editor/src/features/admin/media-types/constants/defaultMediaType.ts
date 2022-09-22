import { ContentTypeName, IMediaTypeModel } from 'hooks';

export const defaultMediaType: IMediaTypeModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  contentType: ContentTypeName.Snippet,
  autoTranscribe: false,
  disableTranscribe: false,
};
