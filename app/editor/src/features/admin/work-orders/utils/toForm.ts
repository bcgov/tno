import { IWorkOrderModel } from 'tno-core';

import { IWorkOrderForm } from '../interfaces';

export const toForm = (model: IWorkOrderModel): IWorkOrderForm => {
  return {
    ...model,
    requestorId: model.requestorId ?? '',
    assignedId: model.assignedId ?? '',
  };
};
