import { IAdvancedSearchFilter } from '../interfaces';

export const defaultAdvancedSearch: IAdvancedSearchFilter = {
  searchTerm: '',
  searchInField: { headline: false, byline: false, storyText: false },
  startDate: '',
  endDate: '',
};
