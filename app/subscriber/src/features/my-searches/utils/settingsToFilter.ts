import { IFilterModel } from 'tno-core';

/** function that extracts the filter settings into a state that can be stored in redux
 * @param filterRow - the filter row to extract the settings from
 * @param searchId - the id of the search that the filter row belongs to, used for updating existing search
 * @param topStoryId - the id of the top story action
 */
export const settingsToFilter = (filterRow: IFilterModel, searchId: number, topStoryId: number) => {
  return {
    actions: filterRow.settings.actions?.find((action) => action.id === topStoryId)
      ? ['Top Story']
      : [],
    contentTypes: filterRow.settings.contentTypes ?? [],
    contributorIds: filterRow.settings.contributorIds,
    edition: filterRow.settings.edition,
    inByline: filterRow.settings.inByline,
    inHeadline: filterRow.settings.inHeadline,
    inStory: filterRow.settings.inStory,
    mediaTypeIds: filterRow.settings.mediaTypeIds,
    page: filterRow.settings.page,
    publishedEndOn: filterRow.settings.endDate,
    publishedStartOn: filterRow.settings.startDate,
    savedSearchId: searchId,
    searchTerm: filterRow.settings.search,
    searchUnpublished: filterRow.settings.searchUnpublished,
    section: filterRow.settings.section,
    sentiment: filterRow.settings.sentiment,
    seriesIds: filterRow.settings.seriesIds,
    sourceIds: filterRow.settings.sourceIds,
    tags: filterRow.settings.tags,
    pageIndex: 0,
    pageSize: 500,
    sort: [],
  };
};
