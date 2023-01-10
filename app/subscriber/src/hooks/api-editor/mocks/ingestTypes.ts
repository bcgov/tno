import { ContentTypeName, IIngestTypeModel } from '..';

export const mockIngestTypes: IIngestTypeModel[] = [
  {
    id: 1,
    name: 'media',
    description: '',
    sortOrder: 0,
    isEnabled: true,
    contentType: ContentTypeName.Snippet,
    autoTranscribe: false,
    disableTranscribe: false,
  },
];
