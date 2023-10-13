import {
  IdType,
  ITableInternalColumn,
  ITableInternalFilter,
  ITableInternalFooter,
  ITableInternalGrouping,
  ITableInternalHeader,
  ITableInternalOptions,
  ITableInternalPaging,
  ITableInternalRow,
  ITableInternalSorting,
  ITableSort,
} from '.';

export interface ITableInternal<T extends object>
  extends ITableInternalPaging<T>,
    ITableInternalSorting<T>,
    ITableInternalFilter<T>,
    ITableInternalGrouping<T> {
  rowId: keyof T | ((data?: T) => string);
  columns: ITableInternalColumn<T>[];
  data: T[];
  rows: ITableInternalRow<T>[];
  header: ITableInternalHeader<T>;
  footer: ITableInternalFooter<T>;
  options: ITableInternalOptions<T>;
  activeRowId?: IdType<T>;
  activeRow: ITableInternalRow<T> | null;
  refreshRows: (rows?: ITableInternalRow<T>[]) => void;
  setActiveRow: (rowOrIndex?: null | number | ITableInternalRow<T>) => void;
  toggleSelectedRow: (row: ITableInternalRow<T>) => ITableInternalRow<T>[];
  onSelectedChanged: (
    row: ITableInternalRow<T>,
    event: React.ChangeEvent<HTMLInputElement>,
  ) => void;
  setPageSize: (pageSize: number) => void;
  setSortOrder: (sortOrder: ITableSort<T>[]) => void;
}
