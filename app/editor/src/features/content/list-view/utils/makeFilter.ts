import moment from 'moment';
import { IContentFilter } from 'tno-core';

import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { applySortBy, setTimeFrame } from '.';

/**
 * Creates a IContentFilter that can be passed to the API hook endpoint.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns new IContentFilter object.
 */
export const makeFilter = (
  filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
): IContentFilter => {
  const result: IContentFilter & Partial<IContentListAdvancedFilter> = {
    page: filter.pageIndex + 1,
    quantity: filter.pageSize,
    productIds: filter.productIds ?? undefined,
    sourceIds: filter.sourceIds ?? undefined,
    contentTypes: filter.contentTypes ?? undefined,
    ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
    userId: +filter.userId !== 0 ? +filter.userId : undefined,
    hasTopic: filter.hasTopic ? true : undefined,
    includeHidden: filter.includeHidden ? true : undefined,
    onlyHidden: filter.onlyHidden ? true : undefined,
    onlyPublished: filter.onlyPublished ? true : undefined,
    publishedStartOn: filter.startDate
      ? moment(filter.startDate).toISOString()
      : setTimeFrame(filter.timeFrame as number)?.toISOString(),
    publishedEndOn: filter.endDate ? moment(filter.endDate).toISOString() : undefined,
    actions: applyActions(filter),
    sort: applySortBy(filter.sort),
  };

  if (filter.fieldType && filter.searchTerm) (result as any)[filter.fieldType] = filter.searchTerm;
  return result;
};

/**
 * Creates an array of actions from the provided filter information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns An array of actions.
 */
const applyActions = (filter: IContentListFilter) => {
  const actions = [];
  if (filter.onTicker) actions.push('On Ticker');
  if (filter.commentary) actions.push('Commentary');
  if (filter.topStory) actions.push('Top Story');
  if (filter.homepage) actions.push('Homepage');
  return actions;
};
