import { IFilterModel } from 'tno-core';

export const defaultFilter: IFilterModel = {
  id: 0,
  name: '',
  description: '',
  ownerId: undefined,
  query: {},
  settings: {
    searchUnpublished: false,
    size: 0,
    inHeadline: false,
    inByline: false,
    inStory: false,
    sourceIds: [],
    productIds: [],
    seriesIds: [],
    contributorIds: [],
    actions: [],
    contentTypes: [],
    tags: [],
    sentiment: [],
  },
  sortOrder: 0,
  isEnabled: true,
  reports: [],
  folders: [],
};
