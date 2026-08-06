import { type IOptionItem } from 'tno-core';

import { findOptionByValue } from './findOptionByValue';

export const getStepFilterLabel = (filterOptions: IOptionItem[], filterId?: number) => {
  if (!filterId) return '-';
  return findOptionByValue(filterOptions, filterId)?.label ?? `Filter ${filterId}`;
};
