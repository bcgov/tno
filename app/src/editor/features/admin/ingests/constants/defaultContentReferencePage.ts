import { type IPage } from '@/shared';

import { type IIngestContentReference } from '../interfaces';

export const defaultContentReferencePage: IPage<IIngestContentReference> = {
  pageIndex: 0,
  pageSize: 100,
  pageCount: -1,
  items: [],
  total: 0,
};
