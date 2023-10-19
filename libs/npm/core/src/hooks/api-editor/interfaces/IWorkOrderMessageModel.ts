import { IUserModel, WorkOrderStatusName, WorkOrderTypeName } from '..';

export interface IWorkOrderMessageModel {
  id: number;
  workType: WorkOrderTypeName;
  status: WorkOrderStatusName;
  requestorId?: number;
  requestor?: IUserModel;
  assignedId?: number;
  assigned?: IUserModel;
  contentId?: number;
  description: string;
  note: string;
  configuration: any;
}
