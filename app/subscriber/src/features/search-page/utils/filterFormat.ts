import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import moment from 'moment';
import { IActionModel, IFilterSettingsModel } from 'tno-core';

import { getActionFilters } from './getActionFilter';

export const filterFormat = (
  filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
  actions: IActionModel[],
) => {
  const settings: IFilterSettingsModel = {
    actions: getActionFilters(filter, actions),
    contentTypes: filter.contentTypes,
    contributorIds: filter.contributorIds ?? [],
    dateOffset: filter.dateOffset ?? 0,
    edition: filter.edition ?? '',
    endDate: filter.publishedEndOn
      ? filter.publishedEndOn
      : filter.publishedStartOn
      ? `${moment(filter.publishedStartOn).endOf('day')}`
      : undefined,
    from: 0,
    inByline: filter.inByline ?? false,
    inHeadline: filter.inHeadline ?? false,
    inStory: filter.inStory ?? false,
    mediaTypeIds: filter.mediaTypeIds ?? [],
    page: filter.page ?? '',
    search: filter.searchTerm,
    searchUnpublished: filter.searchUnpublished ?? false,
    section: filter.section ?? '',
    sentiment: filter.sentiment ?? [],
    seriesIds: filter.seriesIds ?? [],
    size: 100,
    sourceIds: filter.sourceIds ?? [],
    startDate: !!filter.publishedStartOn ? filter.publishedStartOn : undefined,
    tags: filter.tags ?? [],
    topStory: filter.topStory ?? false,
  };

  return settings;
};
