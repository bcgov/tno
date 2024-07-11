import { naturalSortValue } from 'features/content/list-view/utils/naturalSort';
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
        let value1 = '';
        let value2 = '';
        if (sortBy.id.startsWith('page')) {
          value1 = naturalSortValue(a);
          value2 = naturalSortValue(b);
        } else if (sortBy.id.startsWith('source.')) {
          const source1 = sources.find((s) => s.id === a.sourceId);
          const source2 = sources.find((s) => s.id === b.sourceId);
          value1 = getIn(source1, sortBy.id.replace('source.', ''));
          value2 = getIn(source2, sortBy.id.replace('source.', ''));
        } else {
          value1 = getIn(a, sortBy.id);
          value2 = getIn(b, sortBy.id);
        }

        if (value1 === value2) continue; // Check the next sort by.
        return (value1 > value2 ? 1 : -1) * (sortBy.desc ? -1 : 1);
      }

      return 0;
    };
};
