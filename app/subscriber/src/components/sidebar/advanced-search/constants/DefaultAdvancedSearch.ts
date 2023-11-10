import moment from 'moment';

import { IAdvancedSearchFilter } from '../interfaces';

export const defaultAdvancedSearch: IAdvancedSearchFilter = {
  searchTerm: '',
  inByline: true,
  inHeadline: true,
  inStory: true,
  startDate: moment().startOf('day').subtract('7', 'days').toISOString(),
  endDate: moment().endOf('day').toISOString(),
  topStory: false,
  frontPage: false,
  hasFile: false,
  boldKeywords: true,
  useUnpublished: false,
};
