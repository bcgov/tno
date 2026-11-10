import { IMediaTypeModel } from '..';
import { ListOptionName } from '../constants';
export const mockMediaTypes: IMediaTypeModel[] = [
  {
    id: 1,
    name: 'Snippets',
    description: '',
    sortOrder: 0,
    isEnabled: true,
    autoTranscribe: false,
    settings: {},
    listOption: ListOptionName.Source,
  },
  {
    id: 2,
    name: 'Newspaper',
    description: '',
    sortOrder: 0,
    isEnabled: true,
    autoTranscribe: false,
    settings: {},
    listOption: ListOptionName.Source,
  },
];
