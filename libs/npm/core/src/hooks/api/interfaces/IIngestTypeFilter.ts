import { ISortPageFilter } from './ISortPageFilter';

export interface IIngestTypeFilter extends ISortPageFilter {
  name?: string;
  description?: string;
}
