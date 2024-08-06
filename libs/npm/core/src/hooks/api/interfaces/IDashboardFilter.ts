import { ReportStatusName } from '../constants';
import { ISortPageFilter } from './ISortPageFilter';

export interface IDashboardFilter extends ISortPageFilter {
  name?: string;
  keyword?: string;
  ownerId?: number;
  isPublic?: boolean;
  isEnabled?: boolean;
  startDate?: string;
  endDate?: string;
  status?: ReportStatusName[];
}
