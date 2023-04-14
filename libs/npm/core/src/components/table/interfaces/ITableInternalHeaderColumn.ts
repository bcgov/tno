import { ITableHookColumn, ITableInternalRow } from '.';

export interface ITableInternalHeaderColumn<T extends object>
  extends Omit<ITableHookColumn<T>, 'cell'> {
  index: number;
  name: keyof T | string;
  label: string;
  isVisible: boolean;
  sort: string | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
  showSort: boolean;
  isSorted: boolean;
  isSortedDesc: boolean;
}
