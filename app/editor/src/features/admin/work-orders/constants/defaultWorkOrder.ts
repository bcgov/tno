import { IWorkOrderModel, WorkOrderStatusName, WorkOrderTypeName } from 'hooks';

export const defaultWorkOrder: IWorkOrderModel = {
  id: 0,
  description: '',
  workType: WorkOrderTypeName.Transcription,
  status: WorkOrderStatusName.Submitted,
  note: '',
};
