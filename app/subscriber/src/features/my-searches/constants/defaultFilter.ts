import { IFilterModel } from 'tno-core';

export const defaultFilter: IFilterModel = {
  id: 0,
  name: '',
  description: '',
  ownerId: undefined,
  query: {},
  settings: {
    searchUnpublished: false,
  },
  sortOrder: 0,
  isEnabled: true,
};
