import { ITableGroup } from '..';
import { ITableInternalRow } from '.';

export interface ITableHookGrouping<T extends object> {
  groupBy?:
    | keyof T
    | ((item: ITableInternalRow<T>, index: number, array: ITableInternalRow<T>[]) => string);
  groupHeading?: (group: ITableGroup<ITableInternalRow<T>>) => React.ReactNode;
}
