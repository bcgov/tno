import { ITableInternalColumn } from '.';

export interface ITableInternalFooter<T extends object> {
  columns: ITableInternalColumn<T>[];
}
