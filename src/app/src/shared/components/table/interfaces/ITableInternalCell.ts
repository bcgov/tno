import { ITableInternalColumn, ITableInternalRow } from '.';

export interface ITableInternalCell<T extends object> extends ITableInternalColumn<T> {
  row: ITableInternalRow<T>;
  original: T;
}
