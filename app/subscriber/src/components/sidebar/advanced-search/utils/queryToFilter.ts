import { IContentListFilter } from 'features/content/list-view/interfaces';
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
    arrays: ['contentTypes', 'sourceIds', 'productIds', 'sort'],
    numbers: ['sourceIds', 'productIds'],
  });

  if (!!Object.keys(search).length) {
    return {
      byline: convertTo(search.byline, 'string', filter.byline),
      contentTypes: search.contentTypes,
      sort: search.sort,
      excludeSourceIds: search.excludeSourceIds?.map((v: any) => convertTo(v, 'number', undefined)),
      headline: convertTo(search.headline, 'string', filter.headline),
      keyword: convertTo(search.keyword, 'string', filter.keyword),
      otherSource: convertTo(search.otherSource, 'string', filter.otherSource),
      productIds: search.productIds?.map((v: any) => convertTo(v, 'number', undefined)),
      sourceIds: search.sourceIds?.map((v: any) => convertTo(v, 'number', undefined)),
      storyText: convertTo(search.storyText, 'string', filter.storyText),
    };
  }
  return filter;
};
