import { ISortBy } from 'features/interfaces';
import { getIn } from 'formik';
import { useLookup } from 'store/hooks';
import { IContentModel } from 'tno-core';

import { defaultSort } from '../constants';

/**
 * Provides a function to sort content.
 * Handles issue where source.sortOrder is different that what is indexed.
 * @returns Function to sort content.
 */
export const useSortContent = () => {
  const [{ sources }] = useLookup();

  return (sort: ISortBy[] = defaultSort) =>
    (a: IContentModel, b: IContentModel) => {
      for (var index in sort) {
        const sortBy = sort[index];
        let value1 = getIn(a, sortBy.id) ?? '';
        let value2 = getIn(b, sortBy.id) ?? '';

        if (sortBy.id.startsWith('source.')) {
          const source1 = sources.find((s) => s.id === a.sourceId);
          const source2 = sources.find((s) => s.id === b.sourceId);
          value1 = getIn(source1, sortBy.id.replace('source.', ''));
          value2 = getIn(source2, sortBy.id.replace('source.', ''));
        }

        if (value1 === value2) continue; // Check the next sort by.
        return (value1 > value2 ? 1 : -1) * (sortBy.desc ? -1 : 1);
      }

      return 0;
    };
};
