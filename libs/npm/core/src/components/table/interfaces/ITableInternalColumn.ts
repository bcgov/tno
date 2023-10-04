import { ITableHookColumn, ITableInternalCell } from '.';

export interface ITableInternalColumn<T extends object>
  extends Omit<ITableHookColumn<T>, 'id' | 'sort' | 'showSort' | 'isSorted' | 'isSortedDesc'> {
  index: number;
  accessor?: keyof T | string | ((data: T) => unknown);
  isVisible: boolean;
  cell: (cell: ITableInternalCell<T>) => React.ReactNode;
}
