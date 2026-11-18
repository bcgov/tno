import { ISortPageFilter } from './ISortPageFilter';

export interface ISeriesFilter extends ISortPageFilter {
  name?: string;
  description?: string;
  isOther?: boolean;
}
