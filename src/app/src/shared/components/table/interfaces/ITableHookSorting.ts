import { ITableInternal, ITableSort } from '.';

export interface ITableHookSorting<T extends object> {
  /** Whether to show the sorting */
  showSort?: boolean;
  /** An array of sort to apply to the data */
  sortOrder?: ITableSort<T>[];
  /** An event that fires when the sort changes */
  onSortChange?: (sort: ITableSort<T>[], table: ITableInternal<T>) => void;
}
