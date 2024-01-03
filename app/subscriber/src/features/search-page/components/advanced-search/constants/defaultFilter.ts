import { IFilterModel } from 'tno-core';

export const defaultFilter: IFilterModel = {
  name: '',
  query: {},
  settings: {
    searchUnpublished: false,
    size: 0,
  },
  id: 0,
  sortOrder: 0,
  description: '',
  isEnabled: true,
  reports: [],
  folders: [],
};
