import { IContentFilter } from 'hooks/api-editor';
import moment from 'moment';

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
  const result: IContentFilter = {
    page: filter.pageIndex + 1,
    quantity: filter.pageSize,
    sourceId: filter.sourceId !== 0 ? filter.sourceId : undefined,
    productIds: filter.productIds ?? undefined,
    contentType: filter.contentType,
    ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
    userId: +filter.userId !== 0 ? +filter.userId : undefined,
    includedInCategory: filter.includedInCategory ? true : undefined,
    includeHidden: filter.includeHidden ? true : undefined,
    publishedStartOn: filter.startDate
      ? moment(filter.startDate).toISOString()
      : setTimeFrame(filter.timeFrame as number)?.toISOString(),
    publishedEndOn: filter.endDate ? moment(filter.endDate).toISOString() : undefined,
    logicalOperator:
      filter.searchTerm !== '' && filter.logicalOperator !== ''
        ? filter.logicalOperator
        : undefined,
    actions: applyActions(filter),
    sort: applySortBy(filter.sort),
  };

  if (!!filter.fieldType)
    (result as any)[(filter?.fieldType as string) ?? 'fake'] =
      filter.searchTerm !== '' ? filter.searchTerm?.trim() : undefined;

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
  return actions;
};
