import { convertTo, fromQueryString } from 'tno-core';

import { fieldTypes } from '../constants';
import { IContentListAdvancedFilter } from '../interfaces';

/**
 * Updates the specified filter with query param values.
 * @param filter The current filter
 * @param queryString URL Query string
 * @returns The new filter.
 */
export const queryToFilterAdvanced = (
  filter: IContentListAdvancedFilter,
  queryString: string,
): IContentListAdvancedFilter => {
  const search = fromQueryString(queryString);

  if (!!Object.keys(search).length) {
    return {
      fieldType: !!search.fieldType
        ? fieldTypes.find((ft) => ft.value === search.fieldType)?.value ?? filter.fieldType
        : filter.fieldType,
      logicalOperator: convertTo(search.logicalOperator, 'string', filter.logicalOperator),
      searchTerm: search.searchTerm ?? filter.searchTerm,
      startDate: search.startDate ?? filter.startDate,
      endDate: search.endDate ?? filter.endDate,
    };
  }
  return filter;
};
