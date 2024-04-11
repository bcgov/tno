import {
  IAuditColumnsModel,
  IUserContentNotificationModel,
  IUserModel,
  IWorkOrderContentModel,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from '..';

export interface IWorkOrderModel extends IAuditColumnsModel {
  id: number;
  workType: WorkOrderTypeName;
  status: WorkOrderStatusName;
  requestorId?: number;
  requestor?: IUserModel;
  assignedId?: number;
  assigned?: IUserModel;
  description: string;
  note: string;
  configuration: any;
  contentId?: number;
  content?: IWorkOrderContentModel;
  userNotifications?: IUserContentNotificationModel[];
}
