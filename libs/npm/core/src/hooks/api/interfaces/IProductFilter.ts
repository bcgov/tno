import { ISortPageFilter } from './ISortPageFilter';

export interface IProductFilter extends ISortPageFilter {
  name?: string;
  ownerId?: number;
  isEnabled?: boolean;
  isPublic?: boolean;
  subscriberUserId?: number;
  isAvailableToUserId?: number;
}
