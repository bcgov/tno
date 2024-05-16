import { IPaged, IUserModel } from 'tno-core';

export const defaultPage: IPaged<IUserModel> = {
  page: 1,
  quantity: 100,
  items: [],
  total: 0,
};
