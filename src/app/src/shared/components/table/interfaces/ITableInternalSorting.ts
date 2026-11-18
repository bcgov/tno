import { ITableHookSorting, ITableInternal, ITableSort } from '.';

export interface ITableInternalSorting<T extends object> extends ITableHookSorting<T> {
  showSort: boolean;
  sortOrder: ITableSort<T>[];
  onSortChange: (sort: ITableSort<T>[], table: ITableInternal<T>) => void;
}
