import { ITableHookColumn, ITableInternalRow } from '.';

export interface ITableInternalHeaderColumn<T extends object>
  extends Omit<ITableHookColumn<T>, 'cell'> {
  index: number;
  accessor?: keyof T | string | ((data: T) => unknown);
  label: React.ReactNode;
  isVisible: boolean;
  sort?:
    | keyof T
    | string
    | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
  showSort: boolean;
  isSorted: boolean;
  isSortedDesc: boolean;
}
