import { ITableInternalRow } from '.';

export interface ITableSort<T extends object> {
  /** A way to identify each sort */
  id: string;
  /** The column row index position. */
  index: number;
  /** The field to sort on, or a function */
  sort?:
    | keyof T
    | string
    | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
  /** Whether this sort is currently being applied */
  isSorted: boolean;
  /** Whether this sort is in descending order */
  isSortedDesc: boolean;
}
