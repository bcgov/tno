import { WorkOrderStatusName, WorkOrderTypeName } from '@/shared';

import { type IWorkOrderForm } from '../interfaces';

export const defaultWorkOrder: IWorkOrderForm = {
  id: 0,
  description: '',
  requestorId: '',
  assignedId: '',
  workType: WorkOrderTypeName.Transcription,
  status: WorkOrderStatusName.Submitted,
  note: '',
  configuration: {},
};
