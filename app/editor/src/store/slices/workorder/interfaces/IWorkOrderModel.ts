import {
  IAuditColumnsModel,
  IContentModel,
  IUserModel,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'hooks';

export interface IWorkOrderModel extends IAuditColumnsModel {
  id: number;
  workType: WorkOrderTypeName;
  status: WorkOrderStatusName;
  contentId?: number;
  content?: IContentModel;
  requestorId?: number;
  requestor?: IUserModel;
  assignedId?: number;
  assigned?: IUserModel;
  description: string;
  note: string;
}
