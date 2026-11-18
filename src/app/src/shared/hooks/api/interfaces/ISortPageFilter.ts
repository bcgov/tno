import { IPageFilter } from '.';

export interface ISortPageFilter extends IPageFilter {
  sort?: string[];
}
