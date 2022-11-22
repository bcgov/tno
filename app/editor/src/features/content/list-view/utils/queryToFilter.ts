import { convertTo, fromQueryString } from 'tno-core';

import { IContentListFilter } from '../interfaces';

/**
 * Updates the specified filter with query param values.
 * @param filter The current filter
 * @param queryString URL Query string
 * @returns The new filter.
 */
export const queryToFilter = (
  filter: IContentListFilter,
  queryString: string,
): IContentListFilter => {
  const search = fromQueryString(queryString);

  if (!!Object.keys(search).length) {
    return {
      pageIndex: convertTo(search.pageIndex, 'number', filter.pageIndex),
      pageSize: convertTo(search.pageSize, 'number', filter.pageSize),
      includedInCategory: convertTo(
        search.includedInCategory,
        'boolean',
        filter.includedInCategory,
      ),
      includeHidden: convertTo(search.includeHidden, 'boolean', filter.includeHidden),
      contentType: convertTo(search.contentType, 'string', filter.contentType),
      sourceId: convertTo(search.sourceId, 'number', filter.sourceId),
      otherSource: convertTo(search.otherSource, 'string', filter.otherSource),
      productId: convertTo(search.productId, 'number', filter.productId),
      ownerId: convertTo(search.ownerId, 'number', filter.ownerId),
      userId: convertTo(search.userId, 'number', filter.userId),
      timeFrame: convertTo(search.timeFrame, 'number', filter.timeFrame),
      // Actions
      onTicker: convertTo(search.onTicker, 'string', filter.onTicker),
      commentary: convertTo(search.commentary, 'string', filter.commentary),
      topStory: convertTo(search.topStory, 'string', filter.topStory),
      sort: convertTo(search.sort, 'string', filter.sort),
    };
  }
  return filter;
};
