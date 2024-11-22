import { IFilterSettingsModel } from 'tno-core';

export const filterFormat = (filter: IFilterSettingsModel) => {
  const settings: IFilterSettingsModel = {
    queryType: filter.queryType ?? 'simple-query-string',
    defaultOperator: filter.defaultOperator ?? 'and',
    actions: filter.actions ?? [],
    contentTypes: filter.contentTypes ?? [],
    contentIds: filter.contentIds ?? [],
    contributorIds: filter.contributorIds ?? [],
    contributorNames: filter.contributorNames ?? [],
    dateOffset: filter.dateOffset,
    edition: filter.edition ?? '',
    featured: filter.featured ?? false,
    inByline: filter.inByline ?? false,
    inHeadline: filter.inHeadline ?? false,
    inProgram: filter.inProgram ?? false,
    inStory: filter.inStory ?? false,
    mediaTypeIds: filter.mediaTypeIds ?? [],
    page: filter.page ?? '',
    search: filter.search,
    searchUnpublished: filter.searchUnpublished ?? false,
    section: filter.section ?? '',
    sentiment: filter.sentiment ?? [],
    seriesIds: filter.seriesIds ?? [],
    from: filter.from,
    size: filter.size,
    sourceIds: filter.sourceIds ?? [],
    startDate: !!filter.startDate ? filter.startDate : undefined,
    endDate: !!filter.endDate ? filter.endDate : undefined,
    startPostedDate: !!filter.startPostedDate ? filter.startPostedDate : undefined,
    endPostedDate: !!filter.endPostedDate ? filter.endPostedDate : undefined,
    tags: filter.tags ?? [],
  };
  return settings;
};
