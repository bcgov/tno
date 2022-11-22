import { calcPages } from '../../../utils';
import { IPage } from '.';

export class Page<T> implements IPage<T> {
  pageIndex: number;
  pageSize: number;
  pageCount: number;
  items: T[];
  total: number;

  constructor(pageIndex: number, pageSize: number, items: T[], totalItems: number) {
    this.pageIndex = pageIndex;
    this.pageSize = pageSize;
    this.pageCount = calcPages(pageSize, totalItems);
    this.items = items;
    this.total = totalItems;
  }

  toInterface(): IPage<T> {
    return this as IPage<T>;
  }
}
