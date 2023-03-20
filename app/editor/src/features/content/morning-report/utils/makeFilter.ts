import { IContentListAdvancedFilter } from 'features/content/list-view/interfaces';
import { applySortBy, setTimeFrame } from 'features/content/list-view/utils';
import { IContentFilter } from 'tno-core';

import { IMorningReportFilter } from '../interfaces';

/**
 * Creates a IContentFilter that can be passed to the API hook endpoint.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns new IContentFilter object.
 */
export const makeFilter = (
  filter: IMorningReportFilter & Partial<IContentListAdvancedFilter>,
): IContentFilter => {
  const result: IContentFilter = {
    page: filter.pageIndex + 1,
    quantity: filter.pageSize,
    sourceId: filter.sourceId !== 0 ? filter.sourceId : undefined,
    contentType: filter.contentType,
    ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
    userId: +filter.userId !== 0 ? +filter.userId : undefined,
    includedInTopic: filter.includedInTopic ? true : undefined,
    includeHidden: filter.includeHidden ? true : undefined,
    productIds: filter.productIds ?? undefined,
    sourceIds: filter.sourceIds ?? undefined,
    publishedStartOn: setTimeFrame(filter.timeFrame as number)?.toISOString(),
    actions: applyActions(filter),
    sort: applySortBy(filter.sort),
    logicalOperator:
      filter.searchTerm !== '' && filter.logicalOperator !== ''
        ? filter.logicalOperator
        : undefined,
  };
  if (!!filter.fieldType) {
    const searchTerm = filter.searchTerm?.trim();
    (result as any)[(filter?.fieldType as string) ?? 'fake'] =
      filter.searchTerm !== '' ? searchTerm : undefined;
  }
  return result;
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
