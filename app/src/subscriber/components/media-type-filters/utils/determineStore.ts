import { IContentState } from 'store/slices';

export const determineStore = (target: keyof IContentState) => {
  switch (target) {
    case 'home':
      return 'storeHomeFilter';
    case 'todaysCommentary':
      return 'storeTodaysCommentaryFilter';
    case 'topStories':
      return 'storeTopStoriesFilter';
    case 'searchResults':
      return 'storeSearchResultsFilter';
    default:
      return 'storeHomeFilter';
  }
};
