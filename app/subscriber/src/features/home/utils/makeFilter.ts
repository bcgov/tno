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
    actions: applyActions(filter),
    boldKeywords: filter.boldKeywords,
    byline: filter.byline ?? undefined,
    contentTypes: filter.contentTypes ?? [],
    excludeSourceIds: filter.excludeSourceIds ?? undefined,
    hasFile: filter.hasFile,
    headline: filter.headline ?? '',
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

/**
 * Creates an array of actions from the provided filter information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns An array of actions.
 */
const applyActions = (filter: any) => {
  const actions = [];
  if (filter.onTicker) actions.push('On Ticker');
  if (filter.commentary) actions.push('Commentary');
  if (filter.topStory) actions.push('Top Story');
  return actions;
};
