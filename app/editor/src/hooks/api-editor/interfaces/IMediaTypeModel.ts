import { ContentTypeName } from '../constants';
import { ISortableModel } from '.';

export interface IMediaTypeModel extends ISortableModel<number> {
  contentType: ContentTypeName;
  autoTranscribe: boolean;
  disableTranscribe: boolean;
}
