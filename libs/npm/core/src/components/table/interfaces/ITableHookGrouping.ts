import { ITableGroup } from '..';
import { ITableInternalRow } from '.';

export interface ITableHookGrouping<T extends object> {
  groupBy?: (item: T, index: number, array: T[]) => string;
  groupHeading?: (group: ITableGroup<ITableInternalRow<T>>) => React.ReactNode;
}
