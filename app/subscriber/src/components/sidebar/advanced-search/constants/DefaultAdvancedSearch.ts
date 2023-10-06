import { IAdvancedSearchFilter } from '../interfaces';

export const defaultAdvancedSearch: IAdvancedSearchFilter = {
  searchTerm: '',
  inByline: true,
  inHeadline: true,
  inStory: true,
  startDate: '',
  endDate: '',
  topStory: false,
  frontPage: false,
  hasFile: false,
  boldKeywords: true,
  useUnpublished: false,
};
