import { ContentTypeName, type IIngestTypeModel } from '@/shared';

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
