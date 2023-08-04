import { fromQueryString } from 'tno-core';

/** Function that returns advanced search filter state allowing the the inputs to stay in sync when the user is navigating back */
export const queryToState = (queryString: string) => {
  let searchInField = '';

  if (queryString.includes('headline')) searchInField = 'headline';
  if (queryString.includes('byline')) searchInField = 'byline';
  if (queryString.includes('storyText')) searchInField = 'storyText';
  if (queryString.includes('keyword')) searchInField = '';

  const urlParams = new URLSearchParams(queryString);
  const search = fromQueryString(queryString, {
    arrays: ['sourceIds', 'sentiment'],
    numbers: ['sourceIds', 'sentiment'],
  });

  return {
    searchInField: searchInField,
    searchTerm: urlParams.get(searchInField) || '',
    sourceIds: search.sourceIds?.map((v: any) => Number(v)),
    startDate: urlParams.get('publishedStartOn') || '',
    endDate: urlParams.get('publishedEndOn') || '',
    sentiment: search.sentiment?.map((v: any) => Number(v)),
  };
};
