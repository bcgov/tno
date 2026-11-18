import { type IWorkOrderModel } from '@/shared';

export interface IWorkOrderForm extends Omit<IWorkOrderModel, 'assignedId' | 'requestorId'> {
  assignedId: number | '';
  requestorId: number | '';
}
