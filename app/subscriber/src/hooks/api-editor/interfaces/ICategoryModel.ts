import { CategoryTypeName } from '../constants';
import { ISortableModel } from '.';

export interface ICategoryModel extends ISortableModel<number> {
  categoryType: CategoryTypeName;
  autoTranscribe: boolean;
}
