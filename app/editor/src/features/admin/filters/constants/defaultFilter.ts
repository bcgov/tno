import { IFilterModel } from 'tno-core';

export const defaultFilter: IFilterModel = {
  id: 0,
  name: '',
  description: '',
  ownerId: undefined,
  query: {},
  settings: {},
  sortOrder: 0,
  isEnabled: false,
};
