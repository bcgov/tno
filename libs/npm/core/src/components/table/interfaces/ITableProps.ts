import {
  ITableHookColumn,
  ITableHookFilter,
  ITableHookGrouping,
  ITableHookOptions,
  ITableHookPaging,
  ITableHookSorting,
  ITableStyleProps,
} from '.';

export interface ITableProps<T extends object>
  extends ITableHookOptions<T>,
    ITableHookPaging<T>,
    ITableHookSorting<T>,
    ITableHookFilter<T>,
    ITableHookGrouping<T>,
    ITableStyleProps {
  /** The primary key for the row */
  rowId: keyof T;
  /** An array of data to display */
  data: T[];
  /** An array of columns to control the output */
  columns: ITableHookColumn<T>[];
  isLoading?: boolean;
  showActive?: boolean;
}
