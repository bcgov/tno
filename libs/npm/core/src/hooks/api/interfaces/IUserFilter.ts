import { UserAccountTypeName, UserStatusName } from '../constants';
import { ISortPageFilter } from './ISortPageFilter';

export interface IUserFilter extends ISortPageFilter {
  includeUserId?: number;
  username?: string;
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  isEnabled?: boolean;
  isSubscribedToReportId?: number;
  isSystemAccount?: boolean;
  accountTypes?: UserAccountTypeName[];
  status?: UserStatusName;
  roleName?: string;
  keyword?: string;
}
