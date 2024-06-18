import { WorkOrderStatusName, WorkOrderTypeName } from '../constants';
import { ISortBy } from './iSortBy';

export interface IWorkOrderFilter {
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
  sort?: ISortBy[];
  page?: number;
  quantity?: number;
}
