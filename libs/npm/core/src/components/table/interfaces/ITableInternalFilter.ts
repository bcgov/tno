import { ITableHookFilter, ITableInternal } from '.';

export interface ITableInternalFilter<T extends object> extends ITableHookFilter<T> {
  showFilter: boolean;
  search: string;
  onFilterChange: (search: string, table: ITableInternal<T>) => void;
  applyFilter: (search: string) => void;
}
