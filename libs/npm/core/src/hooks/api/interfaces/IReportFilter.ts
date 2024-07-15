import { ISortPageFilter } from './ISortPageFilter';

export interface IReportFilter extends ISortPageFilter {
  name?: string;
  ownerId?: number;
  isPublic?: boolean;
  isPublicOrOwner?: boolean;
  subscriberUserId?: number;
}
