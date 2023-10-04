import { WorkOrderStatusName, WorkOrderTypeName } from '../constants';

export interface IWorkOrderFilter {
  keywords?: string;
  status?: WorkOrderStatusName;
  workType?: WorkOrderTypeName;
  contentId?: number;
  productIds?: number[];
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
