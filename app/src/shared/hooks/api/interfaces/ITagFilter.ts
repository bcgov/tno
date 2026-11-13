import { ISortPageFilter } from './ISortPageFilter';

export interface ITagFilter extends ISortPageFilter {
  name?: string;
  description?: string;
}
