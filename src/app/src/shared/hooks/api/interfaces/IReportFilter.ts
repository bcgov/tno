import { ISortPageFilter } from './ISortPageFilter';

export interface IReportFilter extends ISortPageFilter {
  name?: string;
  ownerId?: number;
  isPublic?: boolean;
  isEnabled?: boolean;
  isPublicOrOwner?: boolean;
  subscriberUserId?: number;
}
