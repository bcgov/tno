import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import moment from 'moment';
import { IFilterSettingsModel } from 'tno-core';

export const filterFormat = (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
  const settings: IFilterSettingsModel = {
    size: 0,
    startDate: !!filter.startDate ? filter.startDate : undefined,
    endDate: filter.endDate
      ? filter.endDate
      : filter.startDate
      ? `${moment(filter.startDate).endOf('day')}`
      : undefined,
    searchUnpublished: filter.useUnpublished ?? false,
    inHeadline: filter.inHeadline ?? false,
    inByline: filter.inByline ?? false,
    sentiment: filter.sentiment ?? [],
    inStory: filter.inStory ?? false,
    sourceIds: filter.sourceIds ?? [],
    productIds: filter.productIds ?? [],
    search: filter.searchTerm,
    seriesIds: [],
    contributorIds: [],
    actions: [],
    contentTypes: filter.contentTypes,
    tags: [],
  };

  return settings;
};
