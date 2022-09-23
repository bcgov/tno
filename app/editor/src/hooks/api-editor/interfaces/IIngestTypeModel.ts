import { ContentTypeName } from '../constants';
import { ISortableModel } from '.';

export interface IIngestTypeModel extends ISortableModel<number> {
  contentType: ContentTypeName;
  autoTranscribe: boolean;
  disableTranscribe: boolean;
}
