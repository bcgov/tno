import moment from 'moment';
import { IActionModel, IFilterSettingsModel } from 'tno-core';

import { getActionFilters } from './getActionFilter';

export const filterFormat = (filter: IFilterSettingsModel, actions?: IActionModel[]) => {
  const settings: IFilterSettingsModel = {
    actions: getActionFilters(filter, actions ?? []),
    contentTypes: filter.contentTypes ?? [],
    contentIds: filter.contentIds ?? [],
    contributorIds: filter.contributorIds ?? [],
    dateOffset: filter.dateOffset,
    defaultSearchOperator: filter.defaultSearchOperator ?? 'and',
    edition: filter.edition ?? '',
    endDate: filter.endDate
      ? filter.endDate
      : filter.startDate
      ? `${moment(filter.startDate).endOf('day')}`
      : undefined,
    from: 0,
    inByline: filter.inByline ?? false,
    inHeadline: filter.inHeadline ?? false,
    inStory: filter.inStory ?? false,
    mediaTypeIds: filter.mediaTypeIds ?? [],
    page: filter.page ?? '',
    search: filter.search,
    searchUnpublished: filter.searchUnpublished ?? false,
    section: filter.section ?? '',
    sentiment: filter.sentiment ?? [],
    seriesIds: filter.seriesIds ?? [],
    size: filter.size,
    sourceIds: filter.sourceIds ?? [],
    startDate: !!filter.startDate ? filter.startDate : undefined,
    tags: filter.tags ?? [],
    topStory: filter.topStory ?? false,
  };
  return settings;
};
