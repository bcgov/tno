import { IPage } from '../interfaces';

export const defaultPage = <ItemType>(items: ItemType[] = []): IPage<ItemType> => ({
  pageIndex: 0,
  pageSize: !!items && !!items.length ? items.length : 10,
  pageCount: !!items && !!items.length ? 1 : 0,
  items,
  total: items.length,
});
