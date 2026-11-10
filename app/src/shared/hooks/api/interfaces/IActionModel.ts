import { ContentTypeName, ValueType } from '../constants';
import { ISortableModel } from '.';

export interface IActionModel extends ISortableModel<number> {
  defaultValue: string;
  valueType: ValueType;
  valueLabel: string;
  contentTypes: ContentTypeName[];
}
