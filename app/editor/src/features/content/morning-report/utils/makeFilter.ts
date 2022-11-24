import { applySortBy, setTimeFrame } from 'features/content/list-view/utils';
import { IContentFilter } from 'hooks/api-editor';

import { IMorningReportFilter } from '../interfaces';

/**
 * Creates a IContentFilter that can be passed to the API hook endpoint.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns new IContentFilter object.
 */
export const makeFilter = (filter: IMorningReportFilter): IContentFilter => {
  return {
    page: filter.pageIndex + 1,
    quantity: filter.pageSize,
    sourceId: filter.sourceId !== 0 ? filter.sourceId : undefined,
    productId: filter.productId !== 0 ? filter.productId : undefined,
    contentType: filter.contentType,
    ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
    userId: +filter.userId !== 0 ? +filter.userId : undefined,
    includedInCategory: filter.includedInCategory ? true : undefined,
    includeHidden: filter.includeHidden ? true : undefined,
    publishedStartOn: setTimeFrame(filter.timeFrame as number)?.toISOString(),
    actions: applyActions(filter),
    sort: applySortBy(filter.sort),
  };
};

/**
 * Creates an array of actions from the provided filter information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns An array of actions.
 */
const applyActions = (filter: IMorningReportFilter) => {
  const actions = [];
  if (filter.onTicker) actions.push('On Ticker');
  if (filter.commentary) actions.push('Commentary');
  if (filter.topStory) actions.push('Top Story');
  return actions;
};
