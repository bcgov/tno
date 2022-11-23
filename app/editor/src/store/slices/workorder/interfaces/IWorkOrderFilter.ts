import { WorkOrderStatusName, WorkOrderTypeName } from 'hooks';

export interface IWorkOrderFilter {
  status?: WorkOrderStatusName;
  workType?: WorkOrderTypeName;
  contentId?: number;
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
