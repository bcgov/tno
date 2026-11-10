import { type IOptionItem } from 'tno-core';

import { normalizeOptionValue } from './normalizeOptionValue';

export const findOptionByValue = (
  options: IOptionItem[],
  value: unknown,
): IOptionItem | undefined => {
  const normalizedValue = normalizeOptionValue(value);
  return options.find((option) => normalizeOptionValue(option.value) === normalizedValue);
};
