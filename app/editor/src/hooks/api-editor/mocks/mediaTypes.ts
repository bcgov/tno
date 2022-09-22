import { ContentTypeName, IMediaTypeModel } from '..';

export const mockMediaTypes: IMediaTypeModel[] = [
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
