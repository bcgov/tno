import { ISortableModel } from '.';

export interface ICategoryModel extends ISortableModel<number> {
  autoTranscribe: boolean;
}
