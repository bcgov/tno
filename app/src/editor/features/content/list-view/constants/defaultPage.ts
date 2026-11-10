import { type IPage } from 'tno-core';

import { type IContentSearchResult } from '@/editor/store/slices';

export const defaultPage: IPage<IContentSearchResult> = {
  pageIndex: 0,
  pageSize: 500,
  pageCount: -1,
  items: [],
  total: 0,
};
