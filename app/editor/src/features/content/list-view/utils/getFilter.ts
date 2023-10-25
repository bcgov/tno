import moment from 'moment';
import {
  ContentTypeName,
  generateQuery,
  IActionModel,
  IContentFilter,
  IFilterSettingsModel,
} from 'tno-core';

import { AdvancedSearchKeys } from '../constants';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { getActions, getSortBy } from '.';

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
    from: filter.pageIndex * filter.pageSize,
    productIds: filter.productIds ?? undefined,
    sourceIds: filter.sourceIds ?? undefined,
    contentTypes: filter.contentTypes ?? [],
    ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
    userId: +filter.userId !== 0 ? +filter.userId : undefined,
    hasTopic: filter.hasTopic,
    includeHidden: filter.includeHidden ? true : undefined,
    onlyHidden: filter.onlyHidden ? true : undefined,
    onlyPublished: filter.onlyPublished ? true : undefined,
    startDate: filter.startDate ? moment(filter.startDate).toISOString() : undefined,
    endDate: filter.endDate ? moment(filter.endDate).toISOString() : undefined,
    dateOffset:
      filter.startDate || filter.endDate ? undefined : filter.timeFrame ? +filter.timeFrame : 0,
    sort: getSortBy(filter.sort),
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
    settings[filter.fieldType !== AdvancedSearchKeys.Page ? filter.fieldType : 'page'] =
      filter.fieldType === AdvancedSearchKeys.Source ? [+filter.searchTerm] : filter.searchTerm;
  }
  if (settings.contentTypes.length === 0 && settings.contentType) {
    const contentType = settings.contentType as ContentTypeName;
    if (contentType) settings.contentTypes = [contentType];
  }
  const result = generateQuery(settings);
  return result;
};
