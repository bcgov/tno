import { ITableInternalRow } from '.';

export interface ITableSort<T extends object> {
  /** A way to identify each sort */
  id: string;
  /** The field to sort on, or a function */
  sort: string | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
  /** Whether this sort is currently being applied */
  isSorted: boolean;
  /** Whether this sort is in descending order */
  isSortedDesc: boolean;
}
