import { calcPages } from '../../../utils';
import { IPage } from '.';

export class Page<IT> implements IPage<IT> {
  pageIndex: number;
  pageSize: number;
  pageCount: number;
  items: IT[];

  constructor(pageIndex: number, pageSize: number, items: IT[], totalItems: number) {
    this.pageIndex = pageIndex;
    this.pageSize = pageSize;
    this.pageCount = calcPages(pageSize, totalItems);
    this.items = items;
  }
}
