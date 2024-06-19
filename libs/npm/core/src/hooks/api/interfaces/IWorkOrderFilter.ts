import { WorkOrderStatusName, WorkOrderTypeName } from '../constants';

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
  sort?: string[];
  page?: number;
  quantity?: number;
}
