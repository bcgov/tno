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
      hasTopic: convertTo(search.hasTopic, 'boolean', filter.hasTopic),
      includeHidden: convertTo(search.includeHidden, 'boolean', filter.includeHidden),
      onlyHidden: convertTo(search.onlyHidden, 'boolean', filter.onlyHidden),
      onlyPublished: convertTo(search.onlyPublished, 'boolean', filter.onlyPublished),
      otherSource: convertTo(search.otherSource, 'string', filter.otherSource),
      ownerId: convertTo(search.ownerId, 'number', filter.ownerId),
      userId: convertTo(search.userId, 'number', filter.userId),
      timeFrame: convertTo(search.timeFrame, 'number', filter.timeFrame),
      contentTypes: search.contentTypes,
      productIds: search.productIds?.map((v: any) => convertTo(v, 'number', undefined)),
      sourceIds: search.sourceIds?.map((v: any) => convertTo(v, 'number', undefined)),
      // Actions
      onTicker: convertTo(search.onTicker, 'boolean', filter.onTicker),
      commentary: convertTo(search.commentary, 'boolean', filter.commentary),
      topStory: convertTo(search.topStory, 'boolean', filter.topStory),
      sort: convertTo(search.sort, 'string', filter.sort),
    };
  }
  return filter;
};
