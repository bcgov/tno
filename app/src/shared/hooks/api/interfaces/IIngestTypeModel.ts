import { ISortableModel } from '.';
import { ContentTypeName } from '../constants';

export interface IIngestTypeModel extends ISortableModel<number> {
  contentType: ContentTypeName;
  autoTranscribe: boolean;
  disableTranscribe: boolean;
}
