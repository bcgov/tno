import { IWorkOrderModel, WorkOrderTypeName } from 'hooks';

/**
 * Find and return the work order of the specified type.
 * @param workOrders An array of work orders.
 * @param type The type of work order to find.
 * @returns The status of the work order of the specified type.
 */
export const findWorkOrder = (
  workOrders: IWorkOrderModel[] | undefined,
  type: WorkOrderTypeName,
) => {
  return workOrders?.find((i) => i.workType === type);
};
