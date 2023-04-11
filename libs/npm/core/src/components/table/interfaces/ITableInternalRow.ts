import { ITableInternal, ITableInternalCell } from '.';

export interface ITableInternalRow<T extends object> {
  index: number;
  table: ITableInternal<T>;
  rowId: keyof T;
  original: T;
  isSelected: boolean;
  isActive: boolean;
  cells: ITableInternalCell<T>[];
  toggleSelected: (value?: boolean) => void;
}
