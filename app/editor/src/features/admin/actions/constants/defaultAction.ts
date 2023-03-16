import { IActionModel, ValueType } from 'hooks';

export const defaultAction: IActionModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  defaultValue: '',
  valueType: ValueType.Boolean,
  valueLabel: '',
};
