import { IContentListAdvancedFilter } from 'features/content/interfaces';
import { convertTo, fromQueryString } from 'tno-core';

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
      fieldType: filter.fieldType,
      logicalOperator: convertTo(search.logicalOperator, 'string', filter.logicalOperator),
      searchTerm: search.searchTerm ?? filter.searchTerm,
      startDate: search.startDate ?? filter.startDate,
      endDate: search.endDate ?? filter.endDate,
      secondaryFieldType: search.secondaryFieldType ?? filter.secondaryFieldType,
      secondaryLogicalOperator: convertTo(
        search.secondaryLogicalOperator,
        'string',
        filter.secondaryLogicalOperator,
      ),
      secondarySearchTerm: search.secondarySearchTerm ?? filter.secondarySearchTerm,
      secondaryStartDate: search.secondaryStartDate ?? filter.secondaryStartDate,
      secondaryEndDate: search.secondaryEndDate ?? filter.secondaryEndDate,
    };
  }
  return filter;
};
