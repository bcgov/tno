import { ITableHookGrouping, ITableInternalRow, ITableInternalRowGroup } from '.';
import { ITableGroup } from '..';

export interface ITableInternalGrouping<T extends object> extends ITableHookGrouping<T> {
  groupHeading: (group: ITableGroup<ITableInternalRow<T>>) => React.ReactNode;
  groups: ITableInternalRowGroup<ITableInternalRow<T>>[];
}
