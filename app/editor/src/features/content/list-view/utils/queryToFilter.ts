import { IContentListFilter } from 'features/content/interfaces';
import { convertTo, fromQueryString } from 'tno-core';

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
  const search = fromQueryString(queryString, {
    arrays: ['contentTypes', 'sourceIds', 'mediaTypeIds', 'sort'],
    numbers: ['sourceIds', 'mediaTypeIds'],
  });

  if (!!Object.keys(search).length) {
    return {
      pageIndex: convertTo(search.pageIndex, 'number', filter.pageIndex),
      pageSize: convertTo(search.pageSize, 'number', filter.pageSize),
      hasTopic: convertTo(search.hasTopic, 'boolean', filter.hasTopic),
      isHidden: convertTo(search.isHidden, 'boolean', filter.isHidden),
      onlyPublished: convertTo(search.onlyPublished, 'boolean', filter.onlyPublished),
      otherSource: convertTo(search.otherSource, 'string', filter.otherSource),
      ownerId: convertTo(search.ownerId, 'number', filter.ownerId),
      userId: convertTo(search.userId, 'number', filter.userId),
      timeFrame: search.timeFrame ? convertTo(search.timeFrame, 'number', filter.timeFrame) : '',
      startDate: search.startDate ? convertTo(search.startDate, 'string', filter.startDate) : '',
      excludeSourceIds: search.excludeSourceIds?.map((v: any) => convertTo(v, 'number', undefined)),
      contentTypes: search.contentTypes ?? [],
      mediaTypeIds: search.mediaTypeIds?.map((v: any) => convertTo(v, 'number', undefined)),
      sourceIds: search.sourceIds?.map((v: any) => convertTo(v, 'number', undefined)),
      // Actions
      commentary: convertTo(search.commentary, 'boolean', filter.commentary),
      topStory: convertTo(search.topStory, 'boolean', filter.topStory),
      featuredStory: convertTo(search.featuredStory, 'boolean', filter.featuredStory),
      sort: convertTo(search.sort, 'string', filter.sort),
    };
  }
  return filter;
};
