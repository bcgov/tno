import { IFilterModel } from 'tno-core';

export const defaultFilter: IFilterModel = {
  id: 0,
  name: '',
  description: '',
  ownerId: undefined,
  query: {},
  settings: {
    searchUnpublished: false,
    inByline: true,
    inStory: true,
    inHeadline: true,
    size: 0,
    startDate: '',
    endDate: '',
    topStory: false,
    frontPage: true,
    sourceIds: [],
    productIds: [],
    seriesIds: [],
    contributorIds: [],
  },
  sortOrder: 0,
  isEnabled: true,
};
