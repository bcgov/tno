import { IAdvancedSearchFilter } from '../interfaces';

export const defaultAdvancedSearch: IAdvancedSearchFilter = {
  searchTerm: '',
  searchInField: { headline: false, byline: false, storyText: false },
  startDate: '',
  endDate: '',
  topStory: false,
  frontPage: false,
  hasFile: false,
  boldKeywords: true,
  index: 'content',
};
