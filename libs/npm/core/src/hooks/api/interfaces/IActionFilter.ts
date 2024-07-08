import { ISortPageFilter } from './ISortPageFilter';

export interface IActionFilter extends ISortPageFilter {
  name?: string;
  description?: string;
}
