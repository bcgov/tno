import { ITableInternal } from '.';

export interface ITableHookFilter<T extends object> {
  /** Whether to show the filer bar */
  showFilter?: boolean;
  /** A search term to look for in the data */
  search?: string;
  /** An event that fires when the search changes */
  onFilterChange?: (search: string, table: ITableInternal<T>) => void;
}
