import { ISortableModel } from '.';
import { ContentTypeName, ValueType } from '../constants';

export interface IActionModel extends ISortableModel<number> {
  defaultValue: string;
  valueType: ValueType;
  valueLabel: string;
  contentTypes: ContentTypeName[];
}
