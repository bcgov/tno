import { ISortPageFilter } from './ISortPageFilter';

export interface IProductFilter extends ISortPageFilter {
  name?: string;
  ownerId?: number;
  isPublic?: boolean;
  subscriberUserId?: number;
}
