import { IFilterSettingsModel } from 'tno-core';

export const filterFormat = (filter: IFilterSettingsModel) => {
  const settings: IFilterSettingsModel = {
    actions: filter.actions ?? [],
    contentTypes: filter.contentTypes ?? [],
    contentIds: filter.contentIds ?? [],
    contributorIds: filter.contributorIds ?? [],
    dateOffset: filter.dateOffset,
    defaultSearchOperator: filter.defaultSearchOperator ?? 'and',
    edition: filter.edition ?? '',
    featured: filter.featured ?? false,
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
    endDate: !!filter.endDate ? filter.endDate : undefined,
    tags: filter.tags ?? [],
  };
  return settings;
};
