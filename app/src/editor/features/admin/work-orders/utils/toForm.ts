import { type IWorkOrderModel } from 'tno-core';

import { type IWorkOrderForm } from '../interfaces';

export const toForm = (model: IWorkOrderModel): IWorkOrderForm => {
  return {
    ...model,
    requestorId: model.requestorId ?? '',
    assignedId: model.assignedId ?? '',
  };
};
