import { ITableColumn, ITableInternalCell, ITableInternalRow } from '.';

export interface ITableHookColumn<T extends object> extends ITableColumn {
  /** How to access the value for this column */
  accessor?: keyof T | string | ((data: T) => unknown);
  /** A label to display in the header */
  label: React.ReactNode;
  /** A function to control the output of the column when displaying data */
  cell?: (cell: ITableInternalCell<T>) => React.ReactNode;
  /** A field to sort on, or a function */
  sort?:
    | keyof T
    | string
    | ((data: ITableInternalRow<T>) => string | number | boolean | undefined | null);
}
