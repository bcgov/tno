import { ITableGroup } from '..';

export interface ITableInternalRowGroup<T extends object> extends ITableGroup<T> {
  heading: (group: ITableGroup<T>) => React.ReactNode;
}
