import { IActionModel, ValueType } from 'tno-core';

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
