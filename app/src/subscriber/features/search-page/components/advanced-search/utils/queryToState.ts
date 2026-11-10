import { fromQueryString } from 'tno-core';

/** Function that returns advanced search filter state allowing the the inputs to stay in sync when the user is navigating back */
export const queryToState = (queryString: string) => {
  const urlParams = new URLSearchParams(queryString);

  const search = fromQueryString(queryString, {
    arrays: ['sourceIds', 'sentiment', 'contributorIds', 'productIds', 'seriesIds', 'tags'],
    numbers: ['sourceIds', 'sentiment', 'contributorIds', 'productIds', 'seriesIds'],
  });

  return {
    searchTerm: search.searchTerm,
    actions: search.actions,
    boldKeywords: search.boldKeywords === 'true',
    hasFile: search.hasFile === 'true',
    inHeadline: search.inHeadline === 'true',
    inStory: search.inStory === 'true',
    inByline: search.inByline === 'true',
    inProgram: search.inProgram === 'true',
    frontPage: search.mediaTypeIds?.includes(11) || false,
    topStory: search.actions?.includes('topStory') || false,
    sourceIds: search.sourceIds?.map((v: any) => Number(v)),
    mediaTypeIds: search.productId?.map((v: any) => Number(v)),
    startDate: urlParams.get('publishedStartOn') || '',
    endDate: urlParams.get('publishedEndOn') || '',
    sentiment: search.sentiment?.map((v: any) => Number(v)),
    searchUnpublished: search.searchUnpublished === 'true',
    section: search.section,
    seriesIds: search.seriesIds?.map((v: any) => Number(v)),
    page: search.paperPage,
    edition: search.edition,
    contributorIds: search.contributorIds?.map((v: any) => Number(v)),
    tags: search.tags?.map((v: any) => String(v)),
  };
};
