import { ContentTypeName, IIngestTypeModel } from 'tno-core';

export const defaultIngestType: IIngestTypeModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  contentType: ContentTypeName.AudioVideo,
  autoTranscribe: false,
  disableTranscribe: false,
};
