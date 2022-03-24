import { IContentFilter } from 'hooks/api-editor';
import moment from 'moment';

import { IContentListAdvancedFilter, IContentListFilter, ISortBy } from './interfaces';
import { setTimeFrame } from './setTimeFrame';

/**
 * Creates a IContentFilter that can be passed to the API hook endpoint.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns new IContentFilter object.
 */
export const makeFilter = (
  filter: IContentListFilter & IContentListAdvancedFilter & { sortBy?: ISortBy[] },
): IContentFilter => {
  const advanced = filter as IContentListAdvancedFilter;
  return {
    page: filter.pageIndex + 1,
    quantity: filter.pageSize,
    mediaTypeId: +filter.mediaTypeId > 0 ? +filter.mediaTypeId : undefined,
    ownerId: +filter.ownerId > 0 ? +filter.ownerId : undefined,
    userId: +filter.userId > 0 ? +filter.userId : undefined,
    contentTypeId: filter.contentTypeId !== 0 ? filter.contentTypeId : undefined,
    createdStartOn: advanced.startDate
      ? moment(advanced.startDate).toISOString()
      : setTimeFrame(filter.timeFrame as number)?.toISOString(),
    createdEndOn: advanced.endDate ? moment(advanced.endDate).toISOString() : undefined,
    [(advanced?.fieldType?.value as string) ?? 'fake']:
      advanced.searchTerm !== '' ? advanced.searchTerm : undefined,
    logicalOperator:
      advanced.searchTerm !== '' && advanced.logicalOperator !== ''
        ? advanced.logicalOperator
        : undefined,
    actions: applyActions(filter),
    sort: applySortBy(filter.sortBy),
  };
};

/**
 * Creates an array of actions from the provided filter information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns An array of actions.
 */
const applyActions = (filter: IContentListFilter) => {
  const actions = [];
  if (filter.included) actions.push('Included');
  if (filter.onTicker) actions.push('On Ticker');
  if (filter.commentary) actions.push('Commentary');
  if (filter.topStory) actions.push('Top Story');
  return actions;
};

/**
 * Creates an array of sort parameters from the provided sorting information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param sortBy An array of sort objects.
 * @returns An array of sort parameters.
 */
const applySortBy = (sortBy?: ISortBy[]) => {
  if (sortBy === undefined || sortBy.length === 0) return undefined;

  var sort: string[] = [];
  for (let i = 0; i < sortBy.length; i++) {
    let column = sortBy[i].id;
    sort.push(`${column}${sortBy[i].desc ? ' desc' : ''}`);
    if (column === 'section') {
      sort.push(`page${sortBy[i].desc ? ' desc' : ''}`);
    }
  }
  return sort;
};
