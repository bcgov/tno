import { IWorkOrderModel } from 'tno-core';

export interface IWorkOrderForm extends Omit<IWorkOrderModel, 'assignedId' | 'requestorId'> {
  assignedId: number | '';
  requestorId: number | '';
}
