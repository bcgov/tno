import { fromQueryString } from 'tno-core';

/** Function that returns advanced search filter state allowing the the inputs to stay in sync when the user is navigating back */
export const queryToState = (queryString: string) => {
  let searchInField = { headline: false, byline: false, storyText: false };
  let searchTerm = '';
  const urlParams = new URLSearchParams(queryString);

  if (queryString.includes('headline')) {
    searchInField.headline = true;
    searchTerm = urlParams.get('headline') ?? '';
  }
  if (queryString.includes('byline')) {
    searchInField.byline = true;
    searchTerm = urlParams.get('byline') ?? '';
  }
  if (queryString.includes('storyText')) {
    searchInField.storyText = true;
    searchTerm = urlParams.get('storyText') ?? '';
  }
  if (queryString.includes('keyword')) {
    searchInField = { headline: false, byline: false, storyText: false };
    searchTerm = urlParams.get('keyword') ?? '';
  }

  const search = fromQueryString(queryString, {
    arrays: ['sourceIds', 'sentiment'],
    numbers: ['sourceIds', 'sentiment'],
  });

  return {
    searchInField: searchInField,
    searchTerm: searchTerm,
    actions: search.actions,
    boldKeywords: search.boldKeywords,
    hasFile: search.hasFile === 'true',
    index: search.index,
    frontPage: search.productIds?.includes(11) || false,
    topStory: search.actions?.includes('Top Story') || false,
    sourceIds: search.sourceIds?.map((v: any) => Number(v)),
    productIds: search.productIds,
    startDate: urlParams.get('publishedStartOn') || '',
    endDate: urlParams.get('publishedEndOn') || '',
    sentiment: search.sentiment?.map((v: any) => Number(v)),
  };
};
