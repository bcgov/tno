import { ITableColumn, ITableInternalCell, ITableInternalRow } from '.';

export interface ITableHookColumn<T extends object> extends ITableColumn {
  /** A unique id to identify this column */
  name: keyof T | string;
  /** A label to display in the header */
  label: React.ReactNode;
  /** A function to control the output of the column when displaying data */
  cell?: (cell: ITableInternalCell<T>) => React.ReactNode;
  /** A field to sort on, or a function */
  sort?: string | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
}
