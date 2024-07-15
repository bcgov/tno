import { WorkOrderStatusName, WorkOrderTypeName } from '../constants';
import { ISortPageFilter } from './ISortPageFilter';

export interface IWorkOrderFilter extends ISortPageFilter {
  keywords?: string;
  isApproved?: boolean;
  status?: WorkOrderStatusName[];
  workType?: WorkOrderTypeName;
  contentId?: number;
  mediaTypeIds?: number[];
  sourceIds?: number[];
  seriesIds?: number[];
  requestorId?: number;
  assignedId?: number;
  createdOn?: string;
  createdStartOn?: string;
  createdEndOn?: string;
  updatedOn?: string;
  updatedStartOn?: string;
  updatedEndOn?: string;
}
