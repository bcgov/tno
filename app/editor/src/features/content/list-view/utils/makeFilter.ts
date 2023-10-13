import moment from 'moment';
import {
  generateQuery,
  IActionModel,
  IContentFilter,
  IFilterActionSettingsModel,
  IFilterSettingsModel,
} from 'tno-core';

import { AdvancedSearchKeys } from '../constants';
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
 * Creates a IContentFilter that can be passed to the API hook endpoint.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns new IContentFilter object.
 */
export const getFilter = (
  filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
  actions: IActionModel[],
): IContentFilter => {
  const settings: IFilterSettingsModel = {
    searchUnpublished: !filter.onlyPublished,
    size: filter.pageSize,
    productIds: filter.productIds ?? undefined,
    sourceIds: filter.sourceIds ?? undefined,
    contentTypes: filter.contentTypes ?? [],
    ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
    userId: +filter.userId !== 0 ? +filter.userId : undefined,
    hasTopic: filter.hasTopic,
    includeHidden: filter.includeHidden ? true : undefined,
    onlyHidden: filter.onlyHidden ? true : undefined,
    onlyPublished: filter.onlyPublished ? true : undefined,
    startDate: filter.startDate
      ? moment(filter.startDate).toISOString()
      : setTimeFrame(filter.timeFrame as number)?.toISOString(),
    endDate: filter.endDate
      ? moment(filter.endDate).toISOString()
      : filter.startDate
      ? moment(filter.startDate).endOf('day').toISOString()
      : undefined,
    sort: applySortBy(filter.sort),
    search: filter.searchTerm,
    inHeadline: filter.fieldType === AdvancedSearchKeys.Headline,
    inByline: filter.fieldType === AdvancedSearchKeys.Byline,
    inStory: false,
    actions: getActions(filter, actions),
    seriesIds: [],
    contributorIds: [],
    tags: [],
    sentiment: [],
  };
  if (filter.fieldType && filter.searchTerm) {
    settings[filter.fieldType] =
      filter.fieldType === AdvancedSearchKeys.Source ? [+filter.searchTerm] : filter.searchTerm;
  }
  const result = generateQuery(settings);
  return result;
};

/**
 * Creates an array of actions from the provided filter information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns An array of actions.
 */
const getActions = (filter: IContentListFilter, actions: IActionModel[]) => {
  const result: IFilterActionSettingsModel[] = [];
  if (filter.commentary) {
    const action = actions.find((x) => x.name === 'Commentary');
    if (action)
      result.push({
        id: action.id,
        value: action.valueLabel,
        valueType: action.valueType,
      });
  }
  if (filter.topStory) {
    const action = actions.find((x) => x.name === 'Top Story');
    if (action)
      result.push({
        id: action.id,
        value: action.valueLabel,
        valueType: action.valueType,
      });
  }
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
