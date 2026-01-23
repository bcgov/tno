import { type ISortBy } from 'features/interfaces';
import { getIn } from 'formik';
import { type IContentModel } from 'tno-core';

import { defaultSort } from '../constants';

/**
 * Sorts the specified items.
 * @param sort An array of sort by property paths.
 * @returns How to sort the specified items.
 */
export const sortContent = (sort: ISortBy[] = defaultSort) => {
  return (a: IContentModel, b: IContentModel) => {
    for (const index in sort) {
      const sortBy = sort[index];
      const value1 = getIn(a, sortBy.id) ?? '';
      const value2 = getIn(b, sortBy.id) ?? '';
      if (value1 === value2) continue; // Check the next sort by.
      return (value1 > value2 ? 1 : -1) * (sortBy.desc ? -1 : 1);
    }

    return 0;
  };
};
