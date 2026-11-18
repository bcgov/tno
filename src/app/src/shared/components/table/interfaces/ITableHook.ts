import {
  ITableHookColumn,
  ITableHookFilter,
  ITableHookGrouping,
  ITableHookOptions,
  ITableHookPaging,
  ITableHookSorting,
} from '.';

export interface ITableHook<T extends object> {
  rowId: keyof T | ((data?: T) => string);
  data: T[];
  columns: ITableHookColumn<T>[];
  options: ITableHookOptions<T>;
  paging: ITableHookPaging<T>;
  sorting?: ITableHookSorting<T>;
  filter?: ITableHookFilter<T>;
  grouping?: ITableHookGrouping<T>;
}
