import { IWorkOrderModel, WorkOrderStatusName, WorkOrderTypeName } from 'tno-core';

export const defaultWorkOrder: IWorkOrderModel = {
  id: 0,
  description: '',
  workType: WorkOrderTypeName.Transcription,
  status: WorkOrderStatusName.Submitted,
  note: '',
  configuration: {},
};
