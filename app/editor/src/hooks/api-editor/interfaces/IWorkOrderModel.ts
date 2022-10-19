import { IAuditColumnsModel, WorkOrderStatusName, WorkOrderTypeName } from '..';

export interface IWorkOrderModel extends IAuditColumnsModel {
  id: number;
  workType: WorkOrderTypeName;
  status: WorkOrderStatusName;
  contentId?: number;
  requestorId?: number;
  assignedId?: number;
  description: string;
  note: string;
}
