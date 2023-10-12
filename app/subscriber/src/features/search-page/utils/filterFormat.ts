import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import moment from 'moment';
import { IFilterSettingsModel } from 'tno-core';

export const filterFormat = (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
  const settings: IFilterSettingsModel = {
    startDate: filter.startDate ?? undefined,
    endDate: !filter.endDate ? `${moment(filter.startDate).endOf('day')}` : filter.endDate,
    searchUnpublished: filter.useUnpublished ?? false,
    inHeadline: filter.inHeadline ?? false,
    inByline: filter.inByline ?? false,
    sentiment: filter.sentiment ?? [],
    inStory: filter.inStory ?? false,
    sourceIds: filter.sourceIds ?? [],
    productIds: filter.productIds ?? [],
    search: filter.searchTerm,
    size: 0,
    seriesIds: [],
    contributorIds: [],
    actions: [],
    contentTypes: [],
    tags: [],
  };

  return settings;
};
