import { ITableInternalHeaderColumn } from '.';

export interface ITableInternalHeader<T extends object> {
  columns: ITableInternalHeaderColumn<T>[];
}
