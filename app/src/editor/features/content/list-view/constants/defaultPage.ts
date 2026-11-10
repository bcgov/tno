import { type IContentSearchResult } from 'store/slices';
import { type IPage } from 'tno-core';

export const defaultPage: IPage<IContentSearchResult> = {
  pageIndex: 0,
  pageSize: 500,
  pageCount: -1,
  items: [],
  total: 0,
};
