import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import moment from 'moment';
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
    byline: filter.byline ?? undefined,
    contentTypes: filter.contentTypes ?? [],
    excludeSourceIds: filter.excludeSourceIds ?? undefined,
    headline: filter.headline ?? '',
    keyword: filter.keyword ?? undefined,
    productIds: filter.productIds ?? undefined,
    publishedEndOn: filter.endDate ? filter.endDate : undefined,
    publishedStartOn: filter.startDate ? filter.startDate : undefined,
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
