import { fromQueryString } from 'tno-core';

/** Function that returns advanced search filter state allowing the the inputs to stay in sync when the user is navigating back */
export const queryToState = (queryString: string) => {
  const urlParams = new URLSearchParams(queryString);

  const search = fromQueryString(queryString, {
    arrays: ['sourceIds', 'sentiment'],
    numbers: ['sourceIds', 'sentiment'],
  });

  return {
    searchTerm: search.searchTerm,
    actions: search.actions,
    boldKeywords: search.boldKeywords === 'true',
    hasFile: search.hasFile === 'true',
    inHeadline: search.inHeadline === 'true',
    inStory: search.inStory === 'true',
    inByline: search.inByline === 'true',
    frontPage: search.mediaTypeIds?.includes(11) || false,
    topStory: search.actions?.includes('Top Story') || false,
    sourceIds: search.sourceIds?.map((v: any) => Number(v)),
    mediaTypeIds: search.mediaTypeIds,
    startDate: urlParams.get('publishedStartOn') || '',
    endDate: urlParams.get('publishedEndOn') || '',
    sentiment: search.sentiment?.map((v: any) => Number(v)),
    useUnpublished: search.useUnpublished === 'true',
  };
};
