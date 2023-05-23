import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import moment from 'moment';
import { IContentFilter } from './IContentFilter';

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
    productIds: filter.productIds ?? undefined,
    sourceIds: filter.sourceIds ?? undefined,
    contentTypes: filter.contentTypes ?? undefined,
    ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
    userId: +filter.userId !== 0 ? +filter.userId : undefined,
    status: filter.status ?? undefined,
    keyword: filter.keyword ?? undefined,
    hasTopic: filter.hasTopic ? true : undefined,
    includeHidden: filter.includeHidden ? true : undefined,
    publishedStartOn: filter.startDate ? moment(filter.startDate).toISOString() : undefined,
    publishedEndOn: filter.endDate ? filter.endDate : undefined,
    logicalOperator:
      filter.searchTerm !== '' && filter.logicalOperator !== ''
        ? filter.logicalOperator
        : undefined,
    actions: applyActions(filter),
  };
  if (!!filter.fieldType) {
    const searchTerm =
      filter.fieldType === 'sourceId' ? filter.searchTerm : filter.searchTerm?.trim();
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
const applyActions = (filter: any) => {
  const actions = [];
  if (filter.onTicker) actions.push('On Ticker');
  if (filter.commentary) actions.push('Commentary');
  if (filter.topStory) actions.push('Top Story');
  return actions;
};
