import { IFilterModel } from 'tno-core';

export const defaultFilter: IFilterModel = {
  id: 0,
  name: '',
  description: '',
  ownerId: undefined,
  query: {},
  settings: {
    from: 0,
    size: 100,
    searchUnpublished: false,
    inHeadline: false,
    inByline: false,
    inStory: false,
    sourceIds: [],
    mediaTypeIds: [],
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
