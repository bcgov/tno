import { ITableHookPaging, ITableInternal, ITableInternalRow, ITablePage } from '.';

export interface ITableInternalPaging<T extends object> extends ITableHookPaging<T> {
  page: ITableInternalRow<T>[];
  pageIndex: number;
  pageSize: number;
  pageCount: number;
  pageNumber: () => number;
  manualPaging: boolean;
  pageButtons: number;
  showPaging: boolean;
  canPrevious: () => boolean;
  canNext: () => boolean;
  goFirst: () => number;
  goPrevious: () => number;
  goNext: () => number;
  goLast: () => number;
  goToPageIndex: (pageIndex: number) => boolean;
  onPageChange: (page: ITablePage, table: ITableInternal<T>) => void;
}
