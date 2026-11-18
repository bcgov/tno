import { type IContentSearchResult } from '@/editor/store/slices';
import { type IPage } from '@/shared';

export const defaultPage: IPage<IContentSearchResult> = {
  pageIndex: 0,
  pageSize: 500,
  pageCount: -1,
  items: [],
  total: 0,
};
