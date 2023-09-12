import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { ISubscriberContentFilter } from 'tno-core';

/**
 * Creates a IContentFilter that can be passed to the API hook endpoint.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns new IContentFilter object.
 */
export const makeFilter = (
  filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
): ISubscriberContentFilter => {
  const result: ISubscriberContentFilter = {
    actions: filter.actions,
    boldKeywords: filter.boldKeywords,
    quantity: filter.pageSize,
    byline: filter.byline ?? undefined,
    contentTypes: filter.contentTypes ?? [],
    excludeSourceIds: filter.excludeSourceIds ?? undefined,
    hasFile: filter.hasFile,
    headline: filter.headline ?? '',
    index: filter.index,
    keyword: filter.keyword ?? undefined,
    names: filter.names ?? undefined,
    productIds: filter.productIds ?? undefined,
    publishedEndOn: filter.endDate ? filter.endDate : undefined,
    publishedStartOn: filter.startDate ? filter.startDate : undefined,
    sentiment: filter.sentiment,
    sourceIds: filter.sourceIds ?? undefined,
    status: filter.status ?? undefined,
    storyText: filter.storyText ?? undefined,
  };
  return result;
};
