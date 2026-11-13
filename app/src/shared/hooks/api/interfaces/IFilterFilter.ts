import { ISortPageFilter } from './ISortPageFilter';

export interface IFilterFilter extends ISortPageFilter {
  name?: string;
  ownerId?: number;
}
