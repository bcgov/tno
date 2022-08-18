import { ISortableModel } from '.';

export interface IMediaTypeModel extends ISortableModel<number> {
  autoTranscribe: boolean;
  disableTranscribe: boolean;
}
