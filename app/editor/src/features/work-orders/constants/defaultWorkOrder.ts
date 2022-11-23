import { WorkOrderStatusName, WorkOrderTypeName } from 'hooks';
import { IWorkOrderModel } from 'store/slices/workorder/interfaces';

export const defaultWorkOrder: IWorkOrderModel = {
  id: 0,
  description: '',
  workType: WorkOrderTypeName.Transcription,
  status: WorkOrderStatusName.Submitted,
  note: '',
};
