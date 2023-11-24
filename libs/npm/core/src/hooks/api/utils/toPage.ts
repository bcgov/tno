import { IPage } from '../../../components/grid-table';
import { IPaged } from '../';

export const toPage = <ItemType>(page: IPaged<ItemType>): IPage<ItemType> => {
  const pageSize = page.quantity > 0 ? page.quantity : 10;
  return {
    pageIndex: page.page > 0 ? page.page - 1 : 0,
    pageSize,
    pageCount: Math.ceil(page.total / pageSize),
    items: page.items,
    total: page.total,
  };
};
