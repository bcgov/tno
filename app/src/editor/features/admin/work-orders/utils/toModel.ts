import { type IWorkOrderModel } from 'tno-core';

import { type IWorkOrderForm } from '../interfaces';

export const toModel = (form: IWorkOrderForm): IWorkOrderModel => {
  return {
    ...form,
    requestorId: form.requestorId === '' ? undefined : form.requestorId,
    assignedId: form.assignedId === '' ? undefined : form.assignedId,
  };
};
