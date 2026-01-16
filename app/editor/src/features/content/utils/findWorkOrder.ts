import { IWorkOrderModel, WorkOrderTypeName } from 'tno-core';

/**
 * Find and return the work order of the specified type.
 * @param workOrders An array of work orders.
 * @param type The type of work order to find.
 * @returns The status of the work order of the specified type.
 */
export const findWorkOrder = (
  workOrders: IWorkOrderModel[] | undefined,
  type: WorkOrderTypeName | WorkOrderTypeName[],
) => {
  if (Array.isArray(type)) {
    return workOrders?.find((i) => type.includes(i.workType));
  }
  return workOrders?.find((i) => i.workType === type);
};
